import { userConnection } from '../signal/connection.js';
import { initAuthModal } from './auth.js';
import { initHeaderUI } from './headerUI.js';
import { initBadges, badgeSubscribeEvent } from './badges.js';

document.addEventListener('DOMContentLoaded', async() => {
    initAuthModal();

    badgeSubscribeEvent();

    await userConnection.start()
        .then(() => console.log('SignalR подключён'))
        .catch(err => console.error('SignalR ошибка:', err));

    await initBadges();
    await initHeaderUI();
});