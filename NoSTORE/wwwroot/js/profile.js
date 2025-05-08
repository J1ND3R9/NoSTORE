document.addEventListener('DOMContentLoaded', () => {

    function setActiveLink() {
        const currentPath = window.location.pathname.split('/').pop();
        document.querySelectorAll('.profile-menu a').forEach(link => {
            link.classList.remove('active');
            const linkPath = link.getAttribute('href').split('/').pop();
            if (linkPath === currentPath) {
                link.classList.add('active');
            }
        });
    }

    setActiveLink();

    document.querySelectorAll('.profile-menu a').forEach(link => {
        link.addEventListener('click', async (e) => {
            e.preventDefault();

            const url = link.getAttribute('href');
            const content = document.getElementById('profile-content');
            content.classList.add('content-fadeout');
            try {
                const response = await fetch(url, {
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    }
                });
                const html = await response.text();
                setTimeout(() => {
                    content.innerHTML = html;
                    content.classList.remove('content-fadeout');
                }, 100);
                history.pushState(null, '', url);

                setActiveLink();
            } catch (error) {
                console.error('Ошибка загрузки содержимого: ', error);
            }
        });
    });
    document.getElementById('logout').addEventListener('click', async () => {
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
        const response = await fetch('/api/auth/logout', {
            method: 'POST'
        });
        if (response.ok) {
            localStorage.removeItem('user');
            window.location.href = '/';
        }
    });
});