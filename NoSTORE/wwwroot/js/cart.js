import { userConnection } from './signalConnection.js';

let isTabActive = true;

document.addEventListener('DOMContentLoaded', () => {
    subscribeEvents();
    const products = document.querySelectorAll('.product');
    initCart();
    products.forEach(initProductCart);
    document.addEventListener("visibilitychange", (e) => {
        isTabActive = document.visibilityState === "visible";
    });
});
function initCart() {
    refreshCartUI();
    const expBtn = document.getElementById('expand-all-spc');
    expBtn.addEventListener('click', () => expandAllSPC());
}
function initProductCart(card) {
    const productId = card.dataset.productid;

    const favBtn = card.querySelector('.favorite');
    const remBtn = card.querySelector('.remove');
    const checkBox = card.querySelector('.hidden-checkbox');
    const decBtn = card.querySelector('.decrease');
    const incBtn = card.querySelector('.increase');

    const totalPriceEl = card.querySelector('.total-price-product');
    const priceForOneEl = card.querySelector('.price-for-one');
    const quantityEl = card.querySelector('.quantity');

    fetchFavoriteStatus(productId, favBtn);

    favBtn.addEventListener('click', () => toggleFavorite(favBtn, productId));
    remBtn.addEventListener('click', () => removeFromCart(card, productId));
    checkBox.addEventListener('click', (e) => toggleSelection(card, e.target.checked, productId));
    incBtn.addEventListener('click', () => changeQuantity(card, 1, productId, quantityEl, totalPriceEl, priceForOneEl, incBtn, decBtn));
    decBtn.addEventListener('click', () => changeQuantity(card, -1, productId, quantityEl, totalPriceEl, priceForOneEl, incBtn, decBtn));
}
// === SignalR ===
function subscribeEvents() {
    cartSubscribe("CartChanged");
}
function cartSubscribe(event) {
    connection.on(event, (update) => {
        if (isTabActive) return;
        const action = update.actionType;
        const cartItem = update.cart;
        if (action === 'Updated') {
            updateProductEvent(cartItem);
        } else if (action === 'Removed') {
            
        } else if (action === 'Added') {
            addProductEvent(cartItem);
        }
    })
}

// === API функции ===
async function fetchFavoriteStatus(productId, btn) {
    try {
        const res = await fetch(`/api/apiproduct/${productId}`);
        const data = await res.json();
        if (data.inFavorite) btn.classList.add('active');
    } catch (err) {
        console.error('Ошибка загрузки статуса: ', err);
    }
}

async function toggleFavorite(btn, productId) {
    const url = btn.classList.contains('active')
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
        btn.classList.toggle('active');
    } catch (err) {
        console.error('Ошибка при изменении избранного:', err);
    }
}

async function removeFromCart(card, productId) {
    try {
        const res = await fetch('/api/apiproduct/remove_product_cart', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ productId })
        });
        if (!res.ok) throw new Error('Ошибка со стороны сервера');
        gsap.to(card, {
            opacity: 0,
            y: -10,
            maxHeight: 0,
            padding: 0,
            margin: 0,
            duration: 0.4,
            ease: 'power1.out',
            onComplete: () => {
                card.remove();
                noItemsCheck(document.querySelectorAll('.product'));
                checkSelectedProducts(card, false)
                refreshCartUI();
            }
        });
    } catch (err) {
        console.error('Ошибка при удалении из корзины:', err);
    }
}

async function toggleSelection(card, checked, productId) {
    const url = checked
        ? '/api/apiproduct/select_product_cart'
        : '/api/apiproduct/unselect_product_cart';

    try {
        const res = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ productId })
        });

        if (!res.ok) throw new Error('Ошибка со стороны сервера');
        checkSelectedProducts(card, checked)
        refreshCartUI();
    } catch (err) {
        console.error('Ошибка при выборе товара:', err);
    }
}

async function changeQuantity(card, delta, productId, quantityEl, totalPriceEl, priceForOneEl, incBtn, decBtn) {
    try {
        if (QuantityHandler(card, incBtn, decBtn, delta)) {
            return;
        }
        const res = await fetch('/api/apiproduct/quantity_product_cart', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ productId, quantity: delta })
        });

        if (!res.ok) throw new Error('Ошибка со стороны сервера');

        card.dataset.quantity = parseFloat(card.dataset.quantity) + delta;

        if (parseFloat(card.dataset.quantity) <= 0) {
            gsap.to(card, {
                opacity: 0,
                y: -10,
                maxHeight: 0,
                padding: 0,
                margin: 0,
                duration: 0.4,
                ease: 'power1.out',
                onComplete: () => {
                    card.remove();
                    noItemsCheck(document.querySelectorAll('.product'));
                    refreshCartUI();
                }
            });
        } else {
            quantityEl.textContent = card.dataset.quantity;
            updateProductPrice(card, parseFloat(card.dataset.oneprice) * delta, totalPriceEl, priceForOneEl)
            refreshCartUI();
        }
        syncSelectedProducts(card);
    } catch (err) {
        console.error('Ошибка при выборе товара:', err);
    }
}

// === Вспомогательные функции ===
function updateProductEvent(cart) {
    const card = document.querySelector(`.product[data-productid='${cart.product.id}']`)
    const newCard = renderProduct(cart);
    card.replaceWith(newCard);
    initProductCart(newCard);
    refreshCartUI();
}
function addProductEvent(cart) {
    const card = renderProduct(cart);
    const container = document.querySelector('.products');
    container.insertBefore(card, container.firstChild);
    addInSPC(cart);
    initProductCart(card);
    refreshCartUI();
}
function renderProduct(cart) {
    const productEl = document.createElement('div');
    productEl.className = 'product';
    productEl.dataset.productid = cart.product.id;
    productEl.dataset.price = cart.totalPrice;
    productEl.dataset.quantity = cart.quantity;
    productEl.dataset.quantityproduct = cart.product.quantity;
    productEl.dataset.oneprice = cart.product.finalPrice;
    productEl.innerHTML = `<a href="/product/${cart.product.name}/${cart.product.SEOName}">
                    <div class="main-image">
                            <img src="/photos/products/${cart.product.name}/${cart.product.image}" />
                    </div>
                </a>
                <div class="info">
                    <div class="left-info">
                        <div class="top">
                            <a href="/product/${cart.product.id}/${cart.product.SEOName}" class="title">${cart.product.name}</a>
                        </div>
                        <div class="bottom">
                            <div class="quantity-controller">
                                <button class="decrease">-</button>
                                <span class="quantity-p" style="${parseInt(cart.quantity) >= parseInt(cart.product.quantity) ? 'color:#E36B3F;' : ''}"><span class="quantity">${cart.quantity}</span> шт.</span>
                                <button class="increase" ${parseInt(cart.quantity) >= parseInt(cart.product.quantity) ? 'disabled' : ''} style="${parseInt(cart.quantity) >= parseInt(cart.product.quantity) ? 'opacity: 0;' : ''}">+</button>
                            </div>
                            <div class="devliery">
                                <svg viewBox="0 0 62 42" fill="none" xmlns="http://www.w3.org/2000/svg">
                                    <path d="M7.75016 24.875L5.81266 21H19.3752L17.8252 17.125H5.16683L3.22933 13.25H23.3793L21.8293 9.37498H2.86766L0.645996 5.49998H10.3335C10.3335 4.12969 10.8778 2.81553 11.8468 1.84659C12.8157 0.877656 14.1299 0.333313 15.5002 0.333313H46.5002V10.6666H54.2502L62.0002 21V33.9166H56.8335C56.8335 35.9721 56.017 37.9433 54.5636 39.3967C53.1102 40.8501 51.1389 41.6666 49.0835 41.6666C47.0281 41.6666 45.0568 40.8501 43.6034 39.3967C42.15 37.9433 41.3335 35.9721 41.3335 33.9166H31.0002C31.0002 35.9721 30.1837 37.9433 28.7302 39.3967C27.2768 40.8501 25.3056 41.6666 23.2502 41.6666C21.1947 41.6666 19.2235 40.8501 17.7701 39.3967C16.3167 37.9433 15.5002 35.9721 15.5002 33.9166H10.3335V24.875H7.75016ZM49.0835 37.7916C50.1112 37.7916 51.0968 37.3834 51.8235 36.6567C52.5502 35.93 52.9585 34.9444 52.9585 33.9166C52.9585 32.8889 52.5502 31.9033 51.8235 31.1766C51.0968 30.4499 50.1112 30.0416 49.0835 30.0416C48.0558 30.0416 47.0702 30.4499 46.3435 31.1766C45.6168 31.9033 45.2085 32.8889 45.2085 33.9166C45.2085 34.9444 45.6168 35.93 46.3435 36.6567C47.0702 37.3834 48.0558 37.7916 49.0835 37.7916ZM52.9585 14.5416H46.5002V21H58.0218L52.9585 14.5416ZM23.2502 37.7916C24.2779 37.7916 25.2635 37.3834 25.9902 36.6567C26.7169 35.93 27.1252 34.9444 27.1252 33.9166C27.1252 32.8889 26.7169 31.9033 25.9902 31.1766C25.2635 30.4499 24.2779 30.0416 23.2502 30.0416C22.2225 30.0416 21.2368 30.4499 20.5101 31.1766C19.7834 31.9033 19.3752 32.8889 19.3752 33.9166C19.3752 34.9444 19.7834 35.93 20.5101 36.6567C21.2368 37.3834 22.2225 37.7916 23.2502 37.7916Z" />
                                </svg>
                                <div>
                                    <span class="delivery-time">Завтра</span>&nbspв г.&nbsp<span class="city">Астрахань</span>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="right-info">
                        <div class="buttons">
                            <button class="remove">
                                <svg viewBox="0 0 32 32" fill="none" xmlns="http://www.w3.org/2000/svg">
                                    <path d="M12.0002 4V5.33333H5.3335V8H6.66683V25.3333C6.66683 26.0406 6.94778 26.7189 7.44788 27.219C7.94797 27.719 8.62625 28 9.3335 28H22.6668C23.3741 28 24.0524 27.719 24.5524 27.219C25.0525 26.7189 25.3335 26.0406 25.3335 25.3333V8H26.6668V5.33333H20.0002V4H12.0002ZM9.3335 8H22.6668V25.3333H9.3335V8ZM12.0002 10.6667V22.6667H14.6668V10.6667H12.0002ZM17.3335 10.6667V22.6667H20.0002V10.6667H17.3335Z" />
                                </svg>
                            </button>
                            <button class="favorite">
                                <svg viewBox="0 0 60 57" fill="none" xmlns="http://www.w3.org/2000/svg">
                                    <path id="Vector" d="M30 57L25.65 52.8997C10.2 38.3935 0 28.7951 0 17.0845C0 7.4861 7.26 0 16.5 0C21.72 0 26.73 2.51608 30 6.46104C33.27 2.51608 38.28 0 43.5 0C52.74 0 60 7.4861 60 17.0845C60 28.7951 49.8 38.3935 34.35 52.8997L30 57Z" />
                                </svg>
                            </button>
                        </div>
                        <div class="price">
                            <span class="total-price-product">${formatCurrency(parseInt(cart.totalPrice))}</span>
                            <span class="price-for-one${parseInt(cart.quantity > 1) ? ' visible' : ' '}">${parseInt(cart.quantity) > 1 ? formatCurrency(parseInt(cart.product.finalPrice)) + '/шт.' : ''}</span>
                        </div>
                        <div class="template__checkboxes">
                            <input type="checkbox" class="hidden-checkbox" name="selectedProducts" id="checkbox${cart.product.id}" value="${cart.product.id}" ${cart.isSelected ? 'checked' : ''}/>
                            <label for="checkbox${cart.product.id}" class="checkbox-label"></label>
                        </div>
                    </div>
                    </div>`
    return productEl;
}
function addInSPC(cart) {
    const container = document.getElementById('all-spc');

    const el = document.createElement('div');
    el.className = 'selected-product-cart';
    el.dataset.productid = cart.product.id;
    el.dataset.price = cart.totalPrice;
    el.innerHTML = ` <div>
                        <span class="spc-name">${cart.product.name}</span>
                        <span class="spc-quantity"><span class="spc-quantity-number">${cart.quantity}</span> шт.</span>
                        <span class="spc-price">${cart.totalPrice}</span>
                    </div>`;

    container.appendChild(el);
    const selectedProducts = document.getElementById('all-spc');
    const arrow = document.getElementById('all-spc-arrow');
    if (selectedProducts.classList.contains('visible')) {
        selectedProducts.classList.remove('visible');
        gsap.to(selectedProducts, {
            maxHeight: 0,
            opacity: 0,
            duration: 0.2
        })
        gsap.to(arrow, {
            rotation: 180,
            transformOrigin: "50% 50%",
            duration: 0.2
        });
    }
}
function updateProductPrice(card, delta, totalEl, perOneEl) {
    let total = parseFloat(card.dataset.price) + delta;
    totalEl.textContent = formatCurrency(total);

    card.dataset.price = total;

    if (delta < 0) delta *= -1;
    if (parseFloat(card.dataset.quantity) > 1) {
        perOneEl.textContent = formatCurrency(delta) + '/шт.';
        if (!perOneEl.classList.contains('visible')) {
            perOneEl.style.opacity = 0;
            perOneEl.style.maxHeight = '0px';
            perOneEl.style.transform = 'scale(0)';
            perOneEl.classList.add('visible');

            gsap.to(perOneEl, {
                opacity: 1,
                scale: 1,
                maxHeight: 100,
                duration: 0.3,
                ease: 'power2.out'
            });
        }
    } else {
        gsap.to(perOneEl, {
            opacity: 0,
            scale: 0,
            maxHeight: 0,
            duration: 0.3,
            ease: 'power2.in',
            onComplete: () => {
                perOneEl.textContent = '';
                perOneEl.classList.remove('visible');
            }
        });
    }
}

function noItemsCheck(products) {
    if (products.length === 0) {
        const container = document.getElementById('products-container');
        const cart = document.getElementById('cart-info');
        cart.style.maxHeight = cart.scrollHeight + 'px';
        gsap.to(cart, {
            maxHeight: 0,
            opacity: 0,
            duration: 0.4
        });
        gsap.to(container, {
            opacity: 0,
            duration: 0.2,
            onComplete: () => {
                container.innerHTML = `<a class="no_items" href="/catalog">
                    <div class="main-image">
                        <img src="/svg/nothing.svg" />
                    </div>
                    <div class="info">
                        <div class="left-info">
                            <div class="top">
                                <h1 class="title">У вас ничего нет в корзине!</h1>
                            </div>
                            <div class="bottom">
                                <span class="assortment">Осмотрите наш ассортимент и подберите что-то под себя!</span>
                            </div>
                        </div>
                    </div>
                </a>`
                gsap.to(container, {
                    opacity: 1,
                    duration: 0.2
                });
            }
        });
    }
}

function refreshCartUI() {
    const { totalPrice, selectedCount } = getCartSummary();
    updateCartUI(totalPrice, selectedCount);
}

function getCartSummary() {
    const products = document.querySelectorAll('.product');
    let totalPrice = 0;
    let selectedCount = 0;

    products.forEach(card => {
        const isSelected = card.querySelector('.hidden-checkbox')?.checked;
        const price = parseFloat(card.dataset.price) || 0;

        if (isSelected) {
            totalPrice += price;
            selectedCount += parseFloat(card.dataset.quantity);
        }
    });

    return { totalPrice, selectedCount };
}

function updateCartUI(total, quantity) {
    const priceEl = document.getElementById('total-price');
    const quantityEl = document.getElementById('total-quantity-selected');
    priceEl.textContent = formatCurrency(total);
    quantityEl.textContent = pluralForm(quantity);
    setTotalQuantity();
    priceFormatSPC();
}

function QuantityHandler(card, incBtn, decBtn, delta) {
    if (!incBtn) {
        return false;
    }
    const quantityMax = parseFloat(card.dataset.quantityproduct);
    const quantity = parseFloat(card.dataset.quantity);
    const quantityEl = card.querySelector('.quantity').parentElement;
    if (quantity >= quantityMax && delta > 0) {
        incBtn.disabled = true;
        incBtn.style.opacity = 0;
        gsap.to(quantityEl, {
            color: '#E36B3F',
            duration: 0.1
        });
        return true;
    } else if (quantity + delta >= quantityMax && delta > 0) {
        incBtn.disabled = true;
        incBtn.style.opacity = 0;
        gsap.to(quantityEl, {
            color: '#E36B3F',
            duration: 0.1
        });
        return false;
    } else {
        incBtn.disabled = false;
        incBtn.style.opacity = 1;
        gsap.to(quantityEl, {
            color: '#E0E0E0',
            duration: 0.1
        });
        return false;
    }

}

function checkSelectedProducts(card, checked) {
    const selectedProduct = document.querySelector(`.selected-product-cart[data-productid='${card.dataset.productid}']`);
    const selectedContainer = document.querySelector(".all-selected-products-cart");
    if (!checked) {
        if (selectedContainer.classList.contains('visible')) {
            selectedProduct.style.maxHeight = selectedProduct.scrollHeight + 'px';
            selectedProduct.style.opacity = 1;
            gsap.to(selectedProduct, {
                maxHeight: 0,
                opacity: 0,
                duration: 0.3,
                onComplete: () => {
                    selectedProduct.remove();
                    selectedContainer.style.maxHeight = selectedContainer.scrollHeight + 'px';
                }
            });
        } else {
            selectedProduct.remove();
        }
    } else {
        const newSelectedProduct = document.createElement("div");
        newSelectedProduct.className = "selected-product-cart";
        newSelectedProduct.setAttribute("data-productid", card.dataset.productid);
        newSelectedProduct.setAttribute("data-price", card.dataset.price);
        newSelectedProduct.innerHTML = `
                <div>
                    <span class="spc-name">${card.querySelector('.title').textContent}</span>
                    <span class="spc-quantity">
                        <span class="spc-quantity-number">${card.dataset.quantity}</span> шт.
                    </span>
                    <span class="spc-price">${card.dataset.price}</span>
                </div>
            `;
        selectedContainer.appendChild(newSelectedProduct);
        if (selectedContainer.classList.contains('visible')) {
            gsap.to(selectedContainer, {
                maxHeight: selectedContainer.scrollHeight + 'px',
                duration: 0.3
            });
        }
    }
}

function syncSelectedProducts(card) {
    const productId = card.dataset.productid;
    const currentQuantity = parseFloat(card.dataset.quantity);
    const currentPrice = parseFloat(card.dataset.price);
    const checkbox = card.querySelector('.hidden-checkbox');

    const selectedProduct = document.querySelector(`.selected-product-cart[data-productid='${productId}'][data-price]`);
    if (checkbox.checked && selectedProduct) {
        if (currentQuantity <= 0) {
            checkSelectedProducts(card, false);
            return;
        } 
        const spcQuantityNumber = selectedProduct.querySelector(".spc-quantity-number");
        const spcPrice = selectedProduct.querySelector(".spc-price");
        spcQuantityNumber.textContent = currentQuantity;
        spcPrice.textContent = formatCurrency(currentPrice);
        selectedProduct.dataset.price = currentPrice;
    }
}

function priceFormatSPC() {
    const spcs = document.querySelectorAll('.selected-product-cart');
    spcs.forEach(spc => {
        const price = spc.querySelector('.spc-price');
        price.textContent = formatCurrency(parseFloat(spc.dataset.price));
    });
}

function setTotalQuantity() {
    const products = document.querySelectorAll('.product');
    let quantity = 0;

    products.forEach(card => {
        quantity += parseFloat(card.dataset.quantity);
    });
    document.getElementById('total-quantity-incart').textContent = quantity;
}

function expandAllSPC() {
    const selectedProducts = document.getElementById('all-spc');
    const arrow = document.getElementById('all-spc-arrow');
    if (selectedProducts.classList.contains('visible')) {
        selectedProducts.classList.remove('visible');
        gsap.to(selectedProducts, {
            maxHeight: 0,
            opacity: 0,
            duration: 0.2
        })
        gsap.to(arrow, {
            rotation: 180,
            transformOrigin: "50% 50%",
            duration: 0.2
        });
    } else {
        const height = selectedProducts.scrollHeight;
        selectedProducts.classList.add('visible');
        selectedProducts.style.display = 'flex';
        gsap.to(selectedProducts, {
            maxHeight: height,
            opacity: 1,
            duration: 0.2
        });
        gsap.to(arrow, {
            rotation: 0,
            transformOrigin: "50% 50%",
            duration: 0.2
        });
    }
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
