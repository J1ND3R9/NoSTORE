import { userConnection } from '../signal/connection.js';

let isTabActive = true;

document.addEventListener('DOMContentLoaded', () => {
    compareSubscribeEvent();
    initDom();
    initCategoryButtons();
    document.addEventListener("visibilitychange", (e) => {
        isTabActive = document.visibilityState === "visible";
    });
    const removeButton = document.getElementById('deleteAll');
    removeButton.addEventListener('click', () => removeAll());
});

// === Иницализации ===
function initCategoryButtons() {
    document.querySelectorAll('.category-btn').forEach(button => {
        button.addEventListener('click', async function () {
            const category = this.getAttribute('data-category');
            // Загрузка данных
            const response = await fetch(`/Compare/${encodeURIComponent(category)}`);

            if (response.ok) {
                const html = await response.text();
                gsap.to(document.getElementById('products'), {
                    autoAlpha: 0,
                    duration: 0.1,
                    onComplete: () => {
                        document.getElementById('products').innerHTML = html;
                        initDom();
                        gsap.to(document.getElementById('products'), {
                            autoAlpha: 1,
                            duration: 0.1
                        })
                    }
                })
            } else {
                document.getElementById('products').innerHTML = '<p>Ошибка загрузки данных.</p>';
            }
        });
    });
}

function initDom() {
    initSticky();
    const products = document.querySelectorAll('.product');
    const productsSticky = document.querySelectorAll('.s-product');
    products.forEach(initProduct)
    productsSticky.forEach(initProduct)
}


// Инициализация продукта
function initProduct(card) {
    const productId = card.dataset.productid;
    const favBtn = card.querySelector('.favorite');
    const cartBtn = card.querySelector('.basket');
    const compBtn = card.querySelector('.compare');
    fetchStatus(productId, favBtn, cartBtn, compBtn);

    favBtn.addEventListener('click', () => addInFavorite(productId, favBtn));
    cartBtn.addEventListener('click', () => addInCart(productId, cartBtn));
    compBtn.addEventListener('click', () => removeCompare(productId));
}

// Иницализация sticky продукта 
function initSticky() {
    var productBlock = document.getElementById('productsList');
    var sticky = document.getElementById('comparesSticky');
    sticky.hidden = true;
    const observer = new IntersectionObserver(
        (entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    gsap.to(sticky, {
                        autoAlpha: 0,
                        top: -50,
                        duration: 0.1,
                        onComplete: () => {
                            sticky.hidden = true;
                        }
                    });
                } else {
                    sticky.style.opacity = 0;
                    sticky.hidden = false;
                    gsap.to(sticky, {
                        autoAlpha: 1,
                        top: 0,
                        duration: 0.1
                    });

                }
            });
        },
        {
            root: null,
            threshold: 0
        }
    );
    observer.observe(productBlock);
}

// === Подключение ===

// Подписка к событию
function compareSubscribeEvent() {
    userConnection.on("ComparesChanged", (update) => {
        if (isTabActive) return;
        location.reload();
    })
}


// === API функции ===

// Получение статуса
async function fetchStatus(productId, favBtn, cartBtn, compBtn) {
    try {
        const response = await fetch(`/api/apiproduct/${productId}`);
        const data = await response.json();

        if (data.inFavorite) favBtn.classList.add('active');
        if (data.inCart) cartBtn.classList.add('active');
        if (data.inCompare) compBtn.classList.add('active');
    } catch (err) {
        console.error('Ошибка загрузки статуса: ', err);
    }
}

// Добавление в сравнение
async function removeCompare(productId) {
    const url = '/api/apiproduct/remove_compare';
    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ productId })
        });
        if (!response.ok) throw new Error('Ошибка сервера');
        location.reload();
    } catch (err) {
        console.error('Ошибка сравнения: ', err);
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

// === Вспомогательные функци ===

// Удаление всех сравнений
function removeAll() {
    const products = document.querySelectorAll('.product');
    products.forEach((card) => {
        const productId = card.dataset.productid;
        removeCompare(productId);
    })
}