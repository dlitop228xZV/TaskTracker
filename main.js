// Основной файл JavaScript для главной страницы

document.addEventListener('DOMContentLoaded', () => {
    console.log('Task Tracker приложение загружено');

    // Подсветка активной ссылки в навигации
    const currentPath = window.location.pathname;
    const navLinks = document.querySelectorAll('.nav__link');

    navLinks.forEach(link => {
        if (link.getAttribute('href') === currentPath) {
            link.classList.add('nav__link--active');
        }
    });

    // Здесь будет логика загрузки данных с API
    // loadDashboardData();
});

// Функция для загрузки данных дашборда (будет реализована позже)
async function loadDashboardData() {
    try {
        // Пример будущего API-запроса
        // const response = await fetch('/api/reports/status-summary');
        // const data = await response.json();
        // updateDashboard(data);
    } catch (error) {
        console.error('Ошибка загрузки данных:', error);
    }
}

// Функция обновления дашборда
function updateDashboard(data) {
    // Здесь будет логика обновления статистики на странице
    console.log('Обновление данных дашборда:', data);
}