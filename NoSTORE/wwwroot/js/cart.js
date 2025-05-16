document.addEventListener('DOMContentLoaded', () => {
    const products = document.querySelectorAll('.product');
    initCart();
    products.forEach(initProductCart);
})
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
