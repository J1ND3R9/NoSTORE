// === Страница с избранным ===
import { userConnection } from '../signal/connection.js';

let isTabActive = true;

document.addEventListener('DOMContentLoaded', () => {
    favoriteSubscribeEvent();
    initFavoriteDOM();
    document.addEventListener("visibilitychange", (e) => {
        isTabActive = document.visibilityState === "visible";
    });
});

// === Инициализации ===

// Инициализация DOM
function initFavoriteDOM() {
    const products = document.querySelectorAll('.product');
    products.forEach(initProduct);
    initFavorite();
    initQuantity();
}

// Инициализация избранного
function initFavorite() {
    const selectFav = document.getElementById('selectFavorite');
    const cartSelect = document.getElementById('inCart');

    selectFav.addEventListener('click', () => selectFavorites(selectFav));
    cartSelect.addEventListener('click', () => selectedProductsCart(cartSelect));
}

// Инициализация количества
function initQuantity() {
    const favQEl = document.getElementById('plural');
    const favSumEl = document.getElementById('sum');

    setFavoriteQuantity(favQEl);
    setFavoriteSum(favSumEl);
}

// Инициализация продукта
function initProduct(card) {
    const productId = card.dataset.productid;

    const favBtn = card.querySelector('.favorite');
    const cartBtn = card.querySelector('.basket');

    const checkbox = card.querySelector('input[type=checkbox]')

    cartBtn.addEventListener('click', () => insertInCart(cartBtn, productId));
    favBtn.addEventListener('click', () => removeFromFavorite(favBtn, productId));

    checkbox.addEventListener('change', () => changeCartStatus(document.getElementById('inCart')));

    fetchProductStatus(productId, favBtn, cartBtn);
}

// === Подключение ===

function favoriteSubscribeEvent() {
    userConnection.on('FavoriteChanged', (update) => {
        if (isTabActive) return;
        partialRefresh();
    });
}


// === API функции ===

// Контроллер ответа от сервера
async function handleResponse(response) {
    if (!response.ok) {
        let message = 'Ошибка со стороны сервера';
        try {
            const data = await response.json();
            if (data.error) message = data.error;
        } catch (_) { }
        throw new Error(message);
    }
}

// Получение Partial и обновление
async function partialRefresh() {
    const container = document.querySelector('.list');
    if (!container) return;
    try {
        const response = await fetch('/favorite/getfavoritepartial');
        await handleResponse(response);
        const html = await response.text();
        container.innerHTML = html;

        initFavoriteDOM();
    } catch (err) {
        console.error(err.message);
    }
}

// Получение статус продукции (в корзине, в избранном)
async function fetchProductStatus(productId, favBtn, cartBtn) {
    try {
        const response = await fetch(`/api/apiproduct/${productId}`);
        const data = await response.json();
        if (data.inFavorite) favBtn.classList.add('active');
        if (data.inCart) cartBtn.classList.add('active');
        changeCartStatus(document.getElementById('inCart'));
    } catch (err) {
        console.error('Ошибка загрузки статуса: ', err);
    }
}

// Удаление продукции из избранного
async function removeFromFavorite(favBtn, productId) {
    const url = favBtn.classList.contains('active')
        ? '/api/apiproduct/remove_product_favorite'
        : '/api/apiproduct/add_product_favorite';

    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ productId })
        });
        await handleResponse(response)
        favBtn.classList.toggle('active');
        if (!favBtn.classList.contains('active')) {
            removeProduct(productId);
            changeCartStatus(document.getElementById('inCart'));
        }
    } catch (err) {
        console.error(err.message);
    }
}

// Добавление товара в корзину
async function insertInCart(cartBtn, productId) {
    const url = cartBtn.classList.contains('active') ? '/api/apiproduct/remove_product_cart' : '/api/apiproduct/add_product_cart';
    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ productId })
        });
        await handleResponse(response)
        cartBtn.classList.toggle('active');
        changeCartStatus(document.getElementById('inCart'));
    } catch (err) {
        console.error(err.message);
    }
}

// Добавить товары в корзину (большим количеством)
async function insertSelectedProducts(productIds, cartBtn) {
    try {
        const response = await fetch('/api/apiproduct/add_product_cart', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ ProductId: "", ProductIds: productIds })
        });
        await handleResponse(response)
        changeCartStatus(cartBtn);
    } catch (err) {
        console.error(err.message);
    }
}

// === Вспомогательные функции ===


// Удалить продукт из избранного
function removeProduct(productId) {
    const product = document.querySelector(`.product[data-productid='${productId}']`);
    const container = document.querySelector('.list');
    if (product) {
        const containerHeight = container.offsetHeight;
        container.style.height = containerHeight + 'px';

        document.querySelectorAll('.product').forEach(p => p.classList.add('disable-transitions'));

        product.style.overflow = 'hidden';
        const state = Flip.getState('.product');

        gsap.to(product, {
            autoAlpha: 0,
            scale: 0.8,
            duration: 0.3,
            onComplete: () => {
                product.remove();
                Flip.from(state, {
                    duration: 0.2,
                    absolute: true,
                    ease: 'power1.inOut',
                    onComplete: () => {
                        container.style.height = '';
                        setFavoriteQuantity();
                        setFavoriteSum();
                        requestAnimationFrame(() => {
                            document.querySelectorAll('.product').forEach(p => p.classList.add('disable-transitions'));
                        })
                    }
                });
            }
        });
    }
}

// Изменение статуса верхней кнопки
function changeCartStatus(cartBtn) {
    if (areAllInCart()) {
        cartBtn.classList.add('all');
        animationBasket(cartBtn, "Все в корзине!");
        return;
    }

    const checkboxes = document.querySelectorAll('.product input[type="checkbox"]');
    const isAnyChecked = Array.from(checkboxes).some(checkbox => checkbox.checked);
    if (isAnyChecked) {
        animationBasket(cartBtn, 'В корзину (выделенное)');
    } else {
        animationBasket(cartBtn, 'В корзину (всё)');
    }

}

// Проверка на всё ли в корзине
function areAllInCart() {
    const cartBtns = document.querySelectorAll('.product .basket');
    const areAllInCart = Array.from(cartBtns).every(cart => cart.classList.contains('active'));
    return areAllInCart;
}

// Вставить в корзину выбранные товары
function selectedProductsCart(cartBtn) {
    if (areAllInCart())
        return;
    const products = document.querySelectorAll('.product');
    const checkboxes = document.querySelectorAll('.product input[type="checkbox"]');

    const productIds = [];
    const isAnyChecked = Array.from(checkboxes).some(checkbox => checkbox.checked);
    if (isAnyChecked) {
        products.forEach(card => {
            const checkbox = card.querySelector('input[type="checkbox"]');
            const basketBtn = card.querySelector('.basket')
            if (checkbox.checked) {
                productIds.push(card.dataset.productid);
                basketBtn.classList.add('active');
            }
        });
    } else {
        products.forEach(card => {
            const basketBtn = card.querySelector('.basket')
            productIds.push(card.dataset.productid);
            basketBtn.classList.add('active');
        });
    }
    insertSelectedProducts(productIds, cartBtn);
}

// Верхняя кнопка выбора
function selectFavorites() {
    let selectType = false;
    const el = document.getElementById('selectFavorite');
    if (el.classList.contains('active')) {
        el.classList.remove('active');
        el.textContent = 'Выделить все';
    } else {
        selectType = true;
        el.classList.add('active');
        el.textContent = 'Сбросить все';
    }
    selectProducts(selectType);
}

// Выбрать/сбросить все выборы
function selectProducts(selectBool) {
    const products = document.querySelectorAll('.product');
    products.forEach(card => {
        const checkbox = card.querySelector('input[type="checkbox"]');
        checkbox.checked = selectBool;
    });
    changeCartStatus(document.getElementById('inCart'));
}

// Установить количество избранного
function setFavoriteQuantity() {
    const products = document.querySelectorAll('.product');
    document.getElementById('plural').textContent = pluralForm(products.length);
}

// Установить сумму в избранном
function setFavoriteSum() {
    const sum = getProductSum();
    document.getElementById('sum').textContent = formatCurrency(sum);
}

// Получить сумму всей продукции
function getProductSum() {
    const products = document.querySelectorAll('.product');
    let sum = 0;
    products.forEach(card => {
        const price = parseFloat(card.dataset.price);
        sum += price;
    });
    return sum;
}

// Анимация
function animationBasket(el, text) {
    el.style.whiteSpace = 'nowrap';
    el.style.overflow = 'hidden';
    gsap.to(el, {
        duration: 0.2,
        onComplete: () => {
            el.textContent = text;
            gsap.to(el, {
                width: el.textContent.length * 11.3,
                duration: 0.2
            });
        }
    });
}

// === Вспомогательные утилиты ===
function formatCurrency(value) {
    return new Intl.NumberFormat('ru-RU').format(value) + ' ₽';
}
function pluralForm(count) {
    const remainder10 = count % 10;
    const remainder100 = count % 100;

    if (remainder10 === 1 && remainder100 !== 11) return `${count} товар`;
    if (remainder10 >= 2 && remainder100 <= 4 && (remainder100 < 12 || remainder100 > 14)) return `${count} товара`;

    return `${count} товаров`;
}
