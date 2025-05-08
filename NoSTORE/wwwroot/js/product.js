document.addEventListener('DOMContentLoaded', () => {
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
});