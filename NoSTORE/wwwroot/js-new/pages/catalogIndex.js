// === Каталог ===

document.addEventListener('DOMContentLoaded', () => {
    initCatalogDOM();
    initFilter();
    const checkboxes = document.querySelectorAll('.hidden-checkbox');
    checkboxes.forEach(initCheckbox);
    const button = document.getElementById('show-button');
    //button.addEventListener('click', func);
});

// === Инициализации ===

// Инициализация ДОМ
function initCatalogDOM() {
    const products = document.querySelectorAll('.product');
    products.forEach(initProduct)

}

function initCheckbox(cb) {
    cb.addEventListener('change', showButton);
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

// Инициализация фильтров
function initFilter() {
    const minPriceEl = document.getElementById('minPriceInput');
    const maxPriceEl = document.getElementById('maxPriceInput');
    if (maxPriceEl && minPriceEl) {
        minPriceEl.addEventListener('focusout', () => {
            if (!minPriceEl.value)
                return;
            if (maxPriceEl.value) {
                if (parseInt(minPriceEl.value) > parseInt(maxPriceEl.value)) {
                    const temp = maxPriceEl.value
                    maxPriceEl.value = minPriceEl.value;
                    minPriceEl.value = temp;
                }
            }
            if (parseInt(minPriceEl.value) < parseInt(minPriceEl.min)) {
                minPriceEl.value = minPriceEl.min;
            }
            if (parseInt(maxPriceEl.value) > parseInt(maxPriceEl.max)) {
                maxPriceEl.value = maxPriceEl.max;
            }
        });

        maxPriceEl.addEventListener('focusout', () => {
            if (!maxPriceEl.value)
                return;

            if (minPriceEl.value) {
                if (parseInt(minPriceEl.value) > parseInt(maxPriceEl.value)) {
                    const temp = maxPriceEl.value;
                    maxPriceEl.value = minPriceEl.value;
                    minPriceEl.value = temp;
                }
            }
            if (parseInt(maxPriceEl.value) > parseInt(maxPriceEl.max)) {
                maxPriceEl.value = maxPriceEl.max;
            }
            if (parseInt(minPriceEl.value) < parseInt(minPriceEl.min)) {
                minPriceEl.value = minPriceEl.min;
            }
        });
    }
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

// Фильтрация каталога
async function applyFilters() {
    //const form = document.getElementById('filterForm');

    //const formData = new FormData(form);

    //const filters = {};

    //for (const [key, value] of formData.entries()) {
    //    const parts = key.split('.');

    //    const category = parts[0];
    //    const param = parts[1];

    //    if (!filters[category]) filters[category] = {};
    //    if (!filters[category][param]) filters[category][param] = [];

    //    filters[category][param].push(value);
    //}

    //let minPrice = 0;
    //let maxPrice = 0;

    //try {
    //    const response = await fetch('/catalog/getfilteredproducts', {
    //        method: 'POST',
    //        headers: { 'Content-Type': 'application/json' },
    //        body: JSON.stringify({
    //            dictionary: filters
    //        })
    //    });
    //    const html = await response.text();
    //    document.getElementById('products').innerHTML = html;
    //    initCatalogDOM();
    //} catch (err) {
    //    console.error('Ошибка фильтрации:', err);
    //}
}

// === Вспомогательные функции ===

// Показ кнопки
function showButton(e) {
    const showButton = document.getElementById('show-button');

    const label = document.querySelector(`label[for="${e.target.id}"]`);
    const rect = label.getBoundingClientRect();
    if (showButton.hidden === true) {
        showButton.hidden = false;
    }

    gsap.to(showButton, {
        top: showButton.style.top = `${window.scrollY + rect.top - 15}px`

    });
    showButton.style.left = `${window.scrollX + rect.right + 10}px`;
}

// === Вспомогательные утилиты ===

//// Транслит
//function translit(word) {
//    var answer = '';
//    var converter = {
//        'а': 'a', 'б': 'b', 'в': 'v', 'г': 'g', 'д': 'd',
//        'е': 'e', 'ё': 'e', 'ж': 'zh', 'з': 'z', 'и': 'i',
//        'й': 'y', 'к': 'k', 'л': 'l', 'м': 'm', 'н': 'n',
//        'о': 'o', 'п': 'p', 'р': 'r', 'с': 's', 'т': 't',
//        'у': 'u', 'ф': 'f', 'х': 'h', 'ц': 'c', 'ч': 'ch',
//        'ш': 'sh', 'щ': 'sch', 'ь': '', 'ы': 'y', 'ъ': '',
//        'э': 'e', 'ю': 'yu', 'я': 'ya',

//        'А': 'A', 'Б': 'B', 'В': 'V', 'Г': 'G', 'Д': 'D',
//        'Е': 'E', 'Ё': 'E', 'Ж': 'Zh', 'З': 'Z', 'И': 'I',
//        'Й': 'Y', 'К': 'K', 'Л': 'L', 'М': 'M', 'Н': 'N',
//        'О': 'O', 'П': 'P', 'Р': 'R', 'С': 'S', 'Т': 'T',
//        'У': 'U', 'Ф': 'F', 'Х': 'H', 'Ц': 'C', 'Ч': 'Ch',
//        'Ш': 'Sh', 'Щ': 'Sch', 'Ь': '', 'Ы': 'Y', 'Ъ': '',
//        'Э': 'E', 'Ю': 'Yu', 'Я': 'Ya'
//    };

//    for (var i = 0; i < word.length; ++i) {
//        if (converter[word[i]] == undefined) {
//            answer += word[i];
//        } else {
//            answer += converter[word[i]];
//        }
//    }

//    return answer
//        .replace(/[^\w\s]/g, '')
//        .trim()
//        .toLowerCase()
//        .replace(/\s+/g, '_');

//}