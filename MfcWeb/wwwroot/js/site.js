// Пример JavaScript для изменения цвета при скролле
window.onscroll = function () {
    var navbar = document.querySelector('.navbar');
    if (window.scrollY > 50) {
        navbar.style.backgroundColor = 'rgba(0, 0, 0, 0.8)'; // Пример изменения фона
    } else {
        navbar.style.backgroundColor = 'transparent';
    }
};
