document.addEventListener('DOMContentLoaded', () => {
    const productId = document.getElementById('productSection').dataset.id;

    initSpecs();
    fetchStatus(productId);

    const chartsBtn = document.getElementById('chartsBtn');
    const chartsEl = document.getElementById('charts');

    const favBtn = document.getElementById('favBtn');
    const cartBtn = document.getElementById('cartBtn');
    const compBtn = document.getElementById('compBtn');

    chartsBtn.addEventListener('click', () => chartsVisibilty(chartsEl));

    favBtn.addEventListener('click', () => favorite(favBtn, productId));
    cartBtn.addEventListener('click', () => cart(cartBtn, productId));
    compBtn.addEventListener('click', () => compare(compBtn, productId));

    modalVisibilityController(chartsEl);
    setDates();
});

// === Иницализации ===

// Инициализация разворачивания хар-ков
function initSpecs() {
    const specsButton = document.getElementById('all_specs');
    const specsContainer = document.getElementById('specials');
    if (specsContainer.style.maxHeight < specsContainer.scrollHeight) {
        specsButton.addEventListener('click', () => {
            if (specsContainer.classList.contains('all')) {
                specsContainer.classList.remove('all');
                specsContainer.style.maxHeight = specsContainer.scrollHeight + 'px';
                specsContainer.offsetHeight;
                specsContainer.style.maxHeight = '800px'
                specsButton.textContent = "Развернуть все характеристики";
            } else {
                specsContainer.classList.add('all');
                specsContainer.style.maxHeight = specsContainer.scrollHeight + 'px';
                specsButton.textContent = "Свернуть характеристики";
            }
        });
    } else {
        specsButton.style.display = 'none';
    }
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

// Получить статус продукта
async function fetchStatus(productId) {
    const favBtn = document.getElementById('favBtn');
    const cartBtn = document.getElementById('cartBtn');
    const compBtn = document.getElementById('compBtn');
    try {
        const response = await fetch(`/api/apiproduct/${productId}`);
        const data = await response.json();
        await handleResponse(response);
        if (data.inFavorite) favBtn.classList.add('active');
        if (data.inCompare) compBtn.classList.add('active');
        if (data.inCart) {
            cartBtn.classList.add('active')
            document.getElementById('cartText').textContent = 'В корзине!'
        };
    } catch (err) {
        console.error(err.message);
    }
}

// Добавить/убрать из избранного
async function favorite(btn, productId) {
    const url = btn.classList.contains('active')
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
        await handleResponse(response);
        btn.classList.toggle('active');
    } catch (err) {
        console.error(err.message);
    }
}

// Добавить/убрать из сравнений
async function compare(compBtn, productId) {
    const url = compBtn.classList.contains('active') ? '/api/apiproduct/remove_compare' : '/api/apiproduct/add_compare';
    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ productId })
        });
        if (!response.ok) throw new Error('Ошибка сервера');
        compBtn.classList.toggle('active');
    } catch (err) {
        console.error('Ошибка сравнения: ', err);
    }
}

// Добавить/убрать из корзины
async function cart(cartBtn, productId) {
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
        if (cartBtn.classList.contains('active')) document.getElementById('cartText').textContent = 'В корзинe!'
        else document.getElementById('cartText').textContent = 'В корзину'
    } catch (err) {
        alert('Не удалось изменить корзину');
        console.error('Ошибка корзины: ', err);
    }
}

// === Вспомогательные функции ===

// Видимость чартов (закрытие)
function modalVisibilityController(modal) {
    let isMouseDownOnOverlay = false;

    modal.addEventListener('mousedown', (e) => {
        if (e.target === modal) {
            isMouseDownOnOverlay = true;
        }
    });

    modal.addEventListener('mouseup', (e) => {
        if (isMouseDownOnOverlay && e.target === modal) {
            gsap.to(modal, {
                autoAlpha: 0,
                top: -20,
                duration: 0.2,
                onComplete: () => {
                    modal.style.display = 'none';
                }
            });
        }
        isMouseDownOnOverlay = false;
    });
    document.addEventListener('keydown', (e) => {
        if (modal.style.display === 'flex' && e.key === 'Escape') {
            gsap.to(modal, {
                autoAlpha: 0,
                top: -20,
                duration: 0.2,
                onComplete: () => {
                    modal.style.display = 'none';
                }
            });
        }
    })
}

// Чарты
function chartsVisibilty(charts) {
    charts.style.opacity = 0;
    charts.style.display = 'flex';
    charts.style.top = '-20px';
    gsap.to(charts, {
        autoAlpha: 1,
        top: 0,
        duration: 0.2
    });
}

// Замена на локальную дату
function setDates() {
    const dateEl = document.querySelectorAll('.date');
    dateEl.forEach((el) => {
        const date = new Date(el.dataset.time);
        el.textContent = date.toLocaleDateString();
    })
    
}