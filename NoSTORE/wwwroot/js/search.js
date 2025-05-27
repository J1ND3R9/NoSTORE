let debounceTimer;
let allProducts = [];
let fuse;

document.addEventListener('DOMContentLoaded', () => {
    document.getElementById('searchInput').addEventListener('input', function () {
        const query = this.value.trim();
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(() => {
            if (!query) {
                document.getElementById('searchResult').innerHTML = '';
                document.querySelector(".search-form").classList.remove('active');
                return;
            }
            document.querySelector(".search-form").classList.add('active');
            performLocalSearch(query);
        }, 300);
    });
    clickOnDocument();
    loadAllProducts();
});

function clickOnDocument() {
    document.addEventListener('click', (e) => {
        const searchInput = document.getElementById('searchInput');
        const searchResults = document.getElementById('searchResult');

        let mouseDownInside = false;
        searchResults.style.pointerEvents = 'auto';
        document.addEventListener('mousedown', (e) => {
            mouseDownInside = searchInput.contains(e.target) || searchResults.contains(e.target);
        })

        document.addEventListener('mouseup', (e) => {
            const mouseUpInside = searchInput.contains(e.target) || searchResults.contains(e.target);

            gsap.killTweensOf(searchResults)

            if (!mouseDownInside && !mouseUpInside) {
                gsap.to(searchResults, {
                    autoAlpha: 0,
                    y: -20,
                    duration: 0.15,
                    onComplete: () => {
                        searchResults.style.pointerEvents = 'none';
                    }
                });
            } else {
                gsap.to(searchResults, {
                    autoAlpha: 1,
                    y: 10,
                    duration: 0.15
                });
            }

            mouseDownInside = false;
        });
    });
}

async function loadAllProducts() {
    try {
        const response = await fetch('/api/apiproduct/all');
        if (!response.ok) throw new Error('Ошибка сервера');

        allProducts = await response.json();

        fuse = new Fuse(allProducts, { keys: ['Name', 'Tags', 'Description'] })
    } catch (err) {
        console.error('Ошибка загрузки данных:', err);
    }
}

function performLocalSearch(query) {
    if (!fuse) {
        displayResults([]);
        return;
    }
    const results = fuse.search(query);
    const onlyFiveResults = results.slice(0, 4);
    displayResults(onlyFiveResults.map(result => result.item));
}

function displayResults(results) {
    const container = document.getElementById('searchResult');
    container.innerHTML = '';
    if (results.length === 0) {
        container.innerHTML = '<div class="no-results">Мы ничего не нашли!</div>';
        return;
    }

    const list = document.createElement('div');
    list.className = 'search-results-list';

    results.forEach(product => {
        const item = document.createElement('a');
        item.href = `/product/${product._id}/${product.SEOName}`;
        item.className = 'search-result-item';
        item.innerHTML = `
        <div class='sr'>
            <div class='sr-image'>
                <img src='/photos/products/${product.Name}/${product.Image}'/>
            </div>
            <div class='sr-info'>
                <h2>${product.Name}</h2>
                <h3>${product.Price}</h3>
            </div>
        </div>
        `;
        list.appendChild(item);
    });
    container.appendChild(list);
    gsap.from(".search-result-item", {
        opacity: 0,
        y: 10,
        duration: 0.4,
        stagger: 0.05,
        ease: "power2.out"
    });
}