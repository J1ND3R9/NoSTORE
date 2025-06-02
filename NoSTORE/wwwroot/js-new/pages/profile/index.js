import { initSettings } from './settings.js';

document.addEventListener('DOMContentLoaded', () => {
    const menuBtns = document.querySelectorAll('.profile-menu a');
    menuBtns.forEach(initMenu);
    setActiveLink();
})

// === Иницализации ===

// Инициализация кнопок слева
function initMenu(menu) {
    menu.addEventListener('click', (e) => getPartial(e, menu));
}

// === API функции ===

// Partial
async function getPartial(e, menu) {
    e.preventDefault();
    const url = menu.getAttribute('href');
    const content = document.getElementById('profile-content');
    content.classList.add('content-fadeout');
    try {
        const response = await fetch(url, {
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        });
        const html = await response.text();
        content.innerHTML = html;
        requestAnimationFrame(() => {
            content.classList.remove('content-fadeout');

            // setScript должен быть после вставки DOM
            const currentPath = window.location.pathname.split('/').pop();
            setScript(currentPath);
        });
        history.pushState(null, '', url);
        setActiveLink();
    } catch (error) {
        console.error('Ошибка загрузки содержимого: ', error);
    }
}

// === Вспомогательные функции ===
function setActiveLink() {
    const currentPath = window.location.pathname.split('/').pop();
    document.querySelectorAll('.profile-menu a').forEach(link => {
        link.classList.remove('active');
        const linkPath = link.getAttribute('href').split('/').pop();
        if (linkPath === currentPath) {
            link.classList.add('active');
        }
    });
    setScript(currentPath);
}

function setScript(script) {
    switch (script) {
        case 'settings':
            initSettings();
            break;
    }
}