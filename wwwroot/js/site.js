// Основной файл интерактивности для LMS системы

document.addEventListener('DOMContentLoaded', function () {
    initAnimations();
    initModals();
    initCourseCards();
    initTestInteractivity();
    initLogging();
});

// 1. Анимации при загрузке и скролле
function initAnimations() {
    const animatedElements = document.querySelectorAll('.course-card, .btn, .alert, .table-row');
    
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('fade-in-visible');
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.1 });

    animatedElements.forEach(el => {
        el.classList.add('fade-in-element');
        observer.observe(el);
    });

    // Эффект пульсации для важных кнопок
    const primaryButtons = document.querySelectorAll('.btn-primary');
    primaryButtons.forEach(btn => {
        btn.addEventListener('mouseenter', () => {
            btn.style.transform = 'scale(1.05)';
            btn.style.boxShadow = '0 4px 15px rgba(0, 128, 0, 0.4)';
        });
        btn.addEventListener('mouseleave', () => {
            btn.style.transform = 'scale(1)';
            btn.style.boxShadow = 'none';
        });
    });
}

// 2. Модальные окна для деталей курса (без перезагрузки страницы)
function initModals() {
    // Создаем структуру модального окна, если её нет
    let modalContainer = document.getElementById('courseModalContainer');
    if (!modalContainer) {
        modalContainer = document.createElement('div');
        modalContainer.id = 'courseModalContainer';
        modalContainer.className = 'modal-overlay';
        modalContainer.innerHTML = `
            <div class="modal-content">
                <span class="close-modal">&times;</span>
                <div id="modalBody"></div>
            </div>
        `;
        document.body.appendChild(modalContainer);
    }

    const closeBtn = modalContainer.querySelector('.close-modal');
    closeBtn.onclick = () => modalContainer.style.display = 'none';
    window.onclick = (event) => {
        if (event.target == modalContainer) {
            modalContainer.style.display = 'none';
        }
    };

    // Навешиваем обработчики на кнопки "Подробнее"
    const detailsButtons = document.querySelectorAll('.btn-details');
    detailsButtons.forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.preventDefault();
            const courseId = btn.getAttribute('data-id');
            const courseTitle = btn.getAttribute('data-title');
            const courseDesc = btn.getAttribute('data-desc');
            
            const modalBody = document.getElementById('modalBody');
            modalBody.innerHTML = `
                <h2>${courseTitle}</h2>
                <hr class="my-3">
                <p class="lead">${courseDesc}</p>
                <div class="alert alert-info">
                    <strong>Информация:</strong> Этот курс включает в себя теоретические материалы, 
                    практические примеры решения квадратных уравнений и итоговое тестирование.
                </div>
                <button class="btn btn-success mt-3" onclick="alert('Переход к материалам курса ${courseId}')">
                    Начать изучение
                </button>
            `;
            modalContainer.style.display = 'block';
            
            // Логирование действия
            logUserAction(`Просмотр деталей курса: ${courseTitle}`);
        });
    });
}

// 3. Интерактивность карточек курсов
function initCourseCards() {
    const cards = document.querySelectorAll('.course-card');
    cards.forEach(card => {
        card.addEventListener('mouseenter', () => {
            card.style.transform = 'translateY(-5px)';
            card.style.boxShadow = '0 10px 20px rgba(0,0,0,0.15)';
        });
        card.addEventListener('mouseleave', () => {
            card.style.transform = 'translateY(0)';
            card.style.boxShadow = '0 4px 6px rgba(0,0,0,0.1)';
        });
    });
}

// 4. Интерактивность для тестов (подсветка ответов, таймер)
function initTestInteractivity() {
    const questionBlocks = document.querySelectorAll('.question-block');
    
    questionBlocks.forEach(block => {
        const options = block.querySelectorAll('.form-check-input');
        options.forEach(option => {
            option.addEventListener('change', () => {
                // Сброс стилей у соседей
                options.forEach(opt => {
                    opt.closest('.form-check').style.backgroundColor = 'transparent';
                    opt.closest('.form-check').style.borderRadius = '4px';
                });
                
                // Подсветка выбранного
                if (option.checked) {
                    const parent = option.closest('.form-check');
                    parent.style.backgroundColor = '#e8f5e9'; // Светло-зеленый фон
                    parent.style.borderRadius = '8px';
                    parent.style.transition = 'all 0.3s ease';
                }
            });
        });
    });

    // Если есть форма теста, добавим подтверждение перед отправкой
    const testForm = document.querySelector('form[action*="Test"]');
    if (testForm) {
        testForm.addEventListener('submit', (e) => {
            const confirmed = confirm('Вы уверены, что хотите завершить тест? Результат нельзя будет изменить.');
            if (!confirmed) {
                e.preventDefault();
            } else {
                logUserAction('Завершение тестирования');
            }
        });
    }
}

// 5. Имитация логирования действий пользователя (отправка на сервер через fetch)
function logUserAction(actionName) {
    const url = '/api/logs/create'; // Предполагаемый эндпоинт API
    
    const data = {
        action: actionName,
        timestamp: new Date().toISOString(),
        userAgent: navigator.userAgent
    };

    // Отправляем асинхронно, чтобы не блокировать интерфейс
    fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': getAntiForgeryToken() // Для ASP.NET Core
        },
        body: JSON.stringify(data)
    }).catch(err => console.log('Логирование:', actionName, '(сервер недоступен или API не настроено)'));
    
    console.log('Действие пользователя:', actionName);
}

// Вспомогательная функция для получения токена CSRF в ASP.NET Core
function getAntiForgeryToken() {
    const tokenField = document.querySelector('input[name="__RequestVerificationToken"]');
    return tokenField ? tokenField.value : '';
}

// Утилита для плавного скролла к якорям
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        e.preventDefault();
        const targetId = this.getAttribute('href');
        if (targetId === '#') return;
        
        const targetElement = document.querySelector(targetId);
        if (targetElement) {
            targetElement.scrollIntoView({
                behavior: 'smooth'
            });
        }
    });
});
