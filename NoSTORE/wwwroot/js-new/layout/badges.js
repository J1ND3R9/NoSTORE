// === Количество товаров у SVG иконок ===
import { userConnection } from '../signal/connection.js';
// === Инициализации ===

async function initBadges() {
    const quantities = await getQuantities();

    const favQ = quantities.favorite;
    const cartQ = quantities.cart;
    const compQ = quantities.comp;

    const favIcon = document.querySelector('#favorite-button .icon-wrapper');
    const cartIcon = document.querySelector('#basket-button .icon-wrapper');
    const compIcon = document.querySelector('#compare-button .icon-wrapper');

    if (favQ > 0) {
        setBadge(favIcon, favQ)
    } else {
        removeBadge(favIcon)
    }
    if (cartQ > 0) {
        setBadge(cartIcon, cartQ)
    } else {
        removeBadge(cartIcon);
    }
    if (compQ > 0) {
        setBadge(compIcon, compQ)
    } else {
        removeBadge(compIcon);
    }
}

// === События ===
function badgeSubscribeEvent() {
    userConnection.on('CartChanged', handleUpdate);
    userConnection.on('FavoriteChanged', handleUpdate);
    userConnection.on('ComparesChanged', handleUpdate);
}

function handleUpdate(update) {
    const action = update.actionType;
    if (action === 'Update') return;
    initBadges();
}



// === API функции ===

// Получение количество товара
async function getQuantities() {
    try {
        const response = await fetch('/api/apiproduct/getQuantities');
        const data = await response.json();

        return {
            favorite: parseInt(data.favoriteQuantity),
            cart: parseInt(data.cartQuantity),
            comp: parseInt(data.compareQuantity)
        }
    } catch (err) {
        console.error('Ошибка загрузки статуса: ' + err);
        return {
            favorite: 0,
            cart: 0,
            comp: 0
        }
    }
}

// === Вспомогательные функции ===

// Вставка круга
function setBadge(el, quantity) {
    removeBadge(el);

    const badge = document.createElement('div');
    badge.className = 'badge';
    badge.innerText = quantity;
    el.appendChild(badge);
}

// Удаление круга
function removeBadge(el) {
    const oldBadge = el.querySelector('.badge');
    if (oldBadge) oldBadge.remove();
}

// === Экспорт ===
export { initBadges, badgeSubscribeEvent };