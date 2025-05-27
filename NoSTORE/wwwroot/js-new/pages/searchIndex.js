// === Результаты поиска ===

document.addEventListener('DOMContentLoaded', () => {
    initSearch();
    const products = document.querySelectorAll('.product');
    products.forEach(initProduct)
});

// === Инициализации ===

function initSearch() {
    const pluralSpan = document.getElementById('plural');
    const quantityEl = document.getElementById('quantity');
    pluralSpan.textContent = pluralForm(parseInt(quantityEl.textContent));
}

// Инициализация продукта
function initProduct(card) {

    const productId = card.dataset.productid;
    const favBtn = card.querySelector('.favorite');
    const cartBtn = card.querySelector('.basket');
    fetchStatus(productId, favBtn, cartBtn);

    favBtn.addEventListener('click', () => addInFavorite(productId, favBtn));
    cartBtn.addEventListener('click', () => addInCart(productId, cartBtn));
}

// === API функции ===

// Получение статуса
async function fetchStatus(productId, favBtn, cartBtn) {
    try {
        const response = await fetch(`/api/apiproduct/${productId}`);
        const data = await response.json();

        if (data.inFavorite) favBtn.classList.add('active');
        if (data.inCart) cartBtn.classList.add('active');
    } catch (err) {
        console.error('Ошибка загрузки статуса: ', err);
    }
}

// Добавление в избранное
async function addInFavorite(productId, favBtn) {
    const url = favBtn.classList.contains('active') ? '/api/apiproduct/remove_product_favorite' : '/api/apiproduct/add_product_favorite';
    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ productId })
        });
        if (!response.ok) throw new Error('Ошибка сервера');
        favBtn.classList.toggle('active');
    } catch (err) {
        alert('Не удалось изменить избранное');
        console.error('Ошибка избранного: ', err);
    }
}

// Добавление в корзину
async function addInCart(productId, cartBtn) {
    const url = cartBtn.classList.contains('active') ? '/api/apiproduct/remove_product_cart' : '/api/apiproduct/add_product_cart';
    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ productId })
        });
        if (!response.ok) throw new Error('Ошибка сервера');
        cartBtn.classList.toggle('active');
    } catch (err) {
        alert('Не удалось изменить корзину');
        console.error('Ошибка корзины: ', err);
    }
}



// === Вспомогательные утилиты ===
function pluralForm(count) {
    const remainder10 = count % 10;
    const remainder100 = count % 100;

    if (remainder10 === 1 && remainder100 !== 11) return `товар`;
    if (remainder10 >= 2 && remainder100 <= 4 && (remainder100 < 12 || remainder100 > 14)) return `товара`;

    return `товаров`;
}
