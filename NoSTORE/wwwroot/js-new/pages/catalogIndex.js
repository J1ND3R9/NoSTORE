// === Каталог ===

document.addEventListener('DOMContentLoaded', () => {
    initCatalogDOM();
    initFilter();
    const checkboxes = document.querySelectorAll('.hidden-checkbox');
    checkboxes.forEach(initCheckbox);
    const button = document.getElementById('apply-filters');
    button.addEventListener('click', () => applyFilters());
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
    const compBtn = card.querySelector('.compare');
    fetchStatus(productId, favBtn, cartBtn, compBtn);

    favBtn.addEventListener('click', () => addInFavorite(productId, favBtn));
    cartBtn.addEventListener('click', () => addInCart(productId, cartBtn));
    compBtn.addEventListener('click', () => addInCompare(productId, compBtn));
}

// Инициализация фильтров
function initFilter() {
    const minPriceEl = document.getElementById('minPriceInput');
    const maxPriceEl = document.getElementById('maxPriceInput');
    const button = document.getElementById('apply-filters');
    if (maxPriceEl && minPriceEl) {
        minPriceEl.addEventListener('focusout', (e) => {
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
            if (parseInt(minPriceEl.value) > parseInt(maxPriceEl.max)) {
                minPriceEl.value = maxPriceEl.max;
            }
            showButton(e);
        });

        maxPriceEl.addEventListener('focusout', (e) => {
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
            if (parseInt(maxPriceEl.value) < parseInt(minPriceEl.min)) {
                maxPriceEl.value = minPriceEl.min;
            }
            showButton(e);
        });
    }
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
async function addInCompare(productId, compBtn) {
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
    const checkboxes = document.querySelectorAll('.hidden-checkbox');

    const filters = {};

    const minPriceV = parseInt(document.getElementById('minPriceInput').value);
    const maxPriceV = parseInt(document.getElementById('maxPriceInput').value);

    checkboxes.forEach(cb => {
        if (cb.checked) {
            const name = cb.dataset.name;
            const parts = name.split('.');

            const category = parts[0];
            const param = parts[1];
            if (!filters[category]) filters[category] = {};
            if (!filters[category][param]) filters[category][param] = [];

            filters[category][param].push(cb.value);
        }
    });
    try {
        const response = await fetch('/catalog/getfilteredproducts', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                dictionary: filters,
                minprice: minPriceV,
                maxprice: maxPriceV
            })
        });
        const html = await response.text();
        const container = document.getElementById('products');
        gsap.to(container, {
            autoAlpha: 0,
            duration: 0.1,
            onComplete: () => {
                const button = document.getElementById('apply-filters');
                container.innerHTML = html;
                initCatalogDOM();
                gsap.to(container, {
                    autoAlpha: 1,
                    duration: 0.1
                });
                gsap.to(button, {
                    autoAlpha: 0,
                    duration: 0.1,
                    onComplete: () => {
                        showButton.hidden = true;
                        showButton.style.pointerEvents = 'none';
                    }
                })
            }
        });
    } catch (err) {
        console.error('Ошибка фильтрации:', err);
    }
    //fetch('/catalog/getfilteredproducts', {
    //    method: 'POST',
    //    headers: { 'Content-Type': 'application/json' },
    //    body: JSON.stringify({ dictionary: filters })
    //})
    //    .then(res => res.text())
    //    .then(html => {
    //        document.getElementById('products').innerHTML = html;
    //        initCatalogDOM();
    //    })
    //    .catch(err => console.error('Ошибка фильтрации:', err));

}

// === Вспомогательные функции ===

// Показ кнопки
function showButton(e) {
    const showButton = document.getElementById('apply-filters');

    const label = document.querySelector(`label[for="${e.target.id}"]`);
    let rect;
    if (!label) {
        rect = document.getElementById('maxPriceInput').getBoundingClientRect();
    } else {
        rect = label.getBoundingClientRect();
    }

    const x = window.scrollX + rect.right + 10;
    const y = window.scrollY + rect.top - 15;

    if (showButton.hidden === true) {
        gsap.set(showButton, {
            autoAlpha: 0,
            pointerEvents: "none"
        });
        showButton.hidden = false;
    }

    gsap.to(showButton, {
        autoAlpha: 1,
        left: `${x}px`,
        top: `${y}px`,
        ease: "power2.out",
        duration: 0.4,
        onComplete: () => {
            showButton.style.pointerEvents = 'all';
        }
    });
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