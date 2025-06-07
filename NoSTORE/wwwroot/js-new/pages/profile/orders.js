function initOrders() {
    const createDates = document.querySelectorAll('.createdate');
    const prices = document.querySelectorAll('.price');
    const quantities = document.querySelectorAll('.quantity');
    const deliveryDates = document.querySelectorAll('.deliverydate');

    createDates.forEach((e) => {
        setDate(e, e.dataset.date);
    });

    deliveryDates.forEach((e) => {
        setDate(e, e.dataset.date);
    });

    prices.forEach((e) => {
        e.textContent = formatCurrency(Number(e.dataset.price));
    });

    quantities.forEach((e) => {
        e.textContent = pluralForm(Number(e.dataset.quantity))
    });
}

function formatCurrency(value) {
    return new Intl.NumberFormat('ru-RU').format(value) + ' ₽';
}
function setDate(el, dataDate) {
    const date = new Date(dataDate);
    el.textContent = date.toLocaleDateString('ru-RU');
}
function pluralForm(count) {
    const remainder10 = count % 10;
    const remainder100 = count % 100;

    if (remainder10 === 1 && remainder100 !== 11) return `${count} товар`;
    if (remainder10 >= 2 && remainder100 <= 4 && (remainder100 < 12 || remainder100 > 14)) return `${count} товара`;

    return `${count} товаров`;
}

export { initOrders };