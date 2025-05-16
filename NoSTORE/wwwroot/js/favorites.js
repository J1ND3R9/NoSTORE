document.addEventListener('DOMContentLoaded', () => {
    const products = document.querySelectorAll('.product');
    products.forEach(initProduct);
    initFavorite();
});

function initFavorite() {
    const favQuantityEl = document.getElementById('plural');
    const favSumEl = document.getElementById('sum');

    const selectFav = document.getElementById('selectFavorite');
    const cartSelect = document.getElementById('inCart');

    selectFav.addEventListener('click', () => selectFavorites(selectFav));
    cartSelect.addEventListener('click', () => selectedProductsCart(cartSelect));

    setFavoriteQuantity(favQuantityEl);
    setFavoriteSum(favSumEl);
}

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

// === API функции ===
async function fetchProductStatus(productId, favBtn, cartBtn) {
    try {
        const res = await fetch(`/api/apiproduct/${productId}`);
        const data = await res.json();
        if (data.inFavorite) favBtn.classList.add('active');
        if (data.inCart) cartBtn.classList.add('active');
        changeCartStatus(document.getElementById('inCart'));
    } catch (err) {
        console.error('Ошибка загрузки статуса: ', err);
    }
}

async function removeFromFavorite(favBtn, productId) {
    const url = favBtn.classList.contains('active')
        ? '/api/apiproduct/remove_product_favorite'
        : '/api/apiproduct/add_product_favorite';

    try {
        const res = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ productId })
        });

        if (!res.ok) throw new Error('Ошибка со стороны сервера');
        favBtn.classList.toggle('active');
        if (!favBtn.classList.contains('active')) {
            removeProduct(productId);
            changeCartStatus(document.getElementById('inCart'));
        }
    } catch (err) {
        console.error('Ошибка при изменении избранного:', err);
    }
}

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
        if (!response.ok) throw new Error('Ошибка сервера');
        cartBtn.classList.toggle('active');
        changeCartStatus(document.getElementById('inCart'));
    } catch (err) {
        alert('Не удалось изменить корзину');
        console.error('Ошибка корзины: ', err);
    }
}

async function insertSelectedProducts(productIds, cartBtn) {
    try {
        const res = await fetch('/api/apiproduct/add_product_cart', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ ProductId: "", ProductIds: productIds })
        });
        if (!res.ok) throw new Error('Ошибка сервера');
        changeCartStatus(cartBtn);
    } catch (err) {
        alert('Не удалось изменить корзину');
        console.error('Ошибка корзины: ', err);
    }
}

// === Вспомогательные функции ===
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
function setHeightBlock() {
    const productsBlock = document.getElementById('products');
}
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

function areAllInCart() {
    const cartBtns = document.querySelectorAll('.product .basket');
    const areAllInCart = Array.from(cartBtns).every(cart => cart.classList.contains('active'));
    return areAllInCart;
}
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
function selectProducts(selectBool) {
    const products = document.querySelectorAll('.product');
    products.forEach(card => {
        const checkbox = card.querySelector('input[type="checkbox"]');
        checkbox.checked = selectBool;
    });
    changeCartStatus(document.getElementById('inCart'));
}


function setFavoriteQuantity() {
    const products = document.querySelectorAll('.product');
    document.getElementById('plural').textContent = pluralForm(products.length);
}

function setFavoriteSum() {
    const sum = getProductSum();
    document.getElementById('sum').textContent = formatCurrency(sum);
}

function getProductSum() {
    const products = document.querySelectorAll('.product');
    let sum = 0;
    products.forEach(card => {
        const price = parseFloat(card.dataset.price);
        sum += price;
    });
    return sum;
}

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
