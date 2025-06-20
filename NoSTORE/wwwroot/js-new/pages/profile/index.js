﻿import { initSettings } from './settings.js';
import { initOrders } from './orders.js';

document.addEventListener('DOMContentLoaded', () => {
    adminPanelButton();
    setActiveLink();
    document.addEventListener('click', handleAjaxLinkClick);
})

// === Основной обработчик кликов по AJAX-ссылкам ===
function handleAjaxLinkClick(e) {
    const target = e.target.closest('a');
    if (!target) return;

    const isAjaxLink = target.matches('.profile-menu a') || target.matches('.admin-nav a');
    if (isAjaxLink) {
        e.preventDefault();
        getPartial(e, target);
    }
}

// === Иницализации ===

// Инициализация кнопок слева
function initMenu(menu) {
    menu.addEventListener('click', (e) => getPartial(e, menu));
}

// === API функции ===

// Добавление новой вкладки (если пользователь админ)
async function adminPanelButton() {
    try {
        const response = await fetch('/api/admin/check');
        const data = await response.json();

        const links = document.querySelectorAll('.profile-menu a');
        const lastLink = links[links.length - 1];

        if (!data.isAdmin) {
            lastLink.remove();
        } else {
            lastLink.style.display = 'flex';
        }
    } catch (_) {
        const links = document.querySelectorAll('.profile-menu a');
        const lastLink = links[links.length - 1].remove();
    }
}

// Partial
async function getPartial(e, menu) {
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
            const currentPath = window.location.pathname.split('/').pop();
            setScript(currentPath);
        });
        history.pushState(null, '', url);
        setActiveLink();
    } catch (error) {
        console.error('Ошибка загрузки содержимого:', error);
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
        case 'orders':
            initOrders();
            break;
    }
}