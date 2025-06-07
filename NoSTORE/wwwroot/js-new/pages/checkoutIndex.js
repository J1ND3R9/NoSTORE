// === Создание заказа ===
document.addEventListener('DOMContentLoaded', () => {
    const sumEl = document.getElementById('sum');
    const qEl = document.getElementById('quantity-total');
    sumEl.textContent = formatCurrency(Number(sumEl.dataset.sum));
    qEl.textContent = pluralForm(Number(qEl.dataset.quantity));
    initProductContainer();
    initPlaceOrder();
});

// === Инициализации ===

// Инициализация блока с продуктами
function initProductContainer() {
    const containerBtn = document.querySelector('.footer-products');
    const containerPrd = document.querySelector('.products');
    if (containerPrd.scrollHeight > containerPrd.offsetHeight) {
        containerBtn.style.display = 'flex';
        containerPrd.style.paddingBottom = '100px';
        initExpand();
    }
}

// Иниацлизация кнопки "развернуть"
function initExpand() {
    const btn = document.getElementById('expand-all-products');
    const containerPrd = document.querySelector('.products');
    btn.addEventListener('click', () => {
        if (btn.classList.contains('active')) {
            gsap.to(containerPrd, {
                maxHeight: 400,
                duration: 0.1
            });
            btn.classList.remove('active');
            btn.textContent = "Развернуть всё"
        } else {
            gsap.to(containerPrd, {
                maxHeight: containerPrd.scrollHeight,
                duration: 0.1
            });
            btn.classList.add('active');
            btn.textContent = "Свернуть всё"
        }
        
    });
}

// Инициализация кнопки оформления
function initPlaceOrder() {
    const btn = document.getElementById('confirm-order');
    btn.addEventListener('click', async () => {
        try {
            const response = await fetch('/api/order/place', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(checkoutDto)
            });
            const data = await response.json();
            if (!response.ok) throw new Error("Произошла ошибка при оформлении заказа")
            window.location.href = `/order/complete/${data.orderid}`;
        } catch (err) {
            console.error(err.message);
        }
    })
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
