    document.addEventListener('DOMContentLoaded', () => {

    const registerSection = document.getElementById('register-section');
    const accountButton = document.getElementById('no-account');
    const title = document.getElementById('title');
    const description = document.getElementById('description')
    const submitButton = document.getElementById('check-combination');
    const codeSection = document.getElementById('code-section');
    const modal = document.getElementById('login-modal');
    const modalContent = document.getElementById('modal-content');

    let currentStage = 'login'

    document.getElementById('button-logout').addEventListener('click', async () => {
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
        const response = await fetch('/api/auth/logout', {
            method: 'POST'
        });
        if (response.ok) {
            localStorage.removeItem('user');
            location.reload();
        }
    });

    document.getElementById('profile-button').addEventListener('click', async () => {
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
        const response = await fetch('/api/auth/me');
        const data = await response.json()
        if (!data.isAuthenticated) {
            modal.style.display = 'flex';
            setTimeout(() => {
                modal.classList.add('active');
            }, 10);
        } else if (data.isAuthenticated) {
            // тут чё то делать
        } else if (!response.Ok) {
            console.error("Ошибка загрузки данных")
        }
        
    });
    document.getElementById('login-modal').addEventListener('click', (e) => {
        if (modal.style.display === 'flex' && e.target === modal) {
            modal.classList.remove('active');
            setTimeout(() => {
                modal.style.display = 'none'
            }, 200);
        }
    });
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape' && modal.style.display === 'flex') {
            modal.classList.remove('active');
            setTimeout(() => {
                modal.style.display = 'none'
            }, 200);
        }
    })

    accountButton.addEventListener('click', () => {
        submitButton.textContent = "Продолжить";
        if (currentStage != 'register') {
            currentStage = 'register';
            accountButton.textContent = "Есть аккаунт";
            registerSection.style.display = 'flex';
            setTimeout(() => {
                registerSection.classList.add('active');
            }, 10);
        } else {
            currentStage = 'login'
            accountButton.textContent = "Нет аккаунта";
            registerSection.classList.remove('active');
            setTimeout(() => {
                registerSection.style.display = 'none';
            }, 200);
        }

    });

    document.getElementById('cant-login').addEventListener('click', () => {
        // Тут надо реализовать восстановление пароля
    });

    submitButton.addEventListener('click', async (e) => {
        e.preventDefault();
        const email = document.getElementById('email').value;
        if (currentStage === 'register') {
            const response = await fetch('/api/auth/register/send-code', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email })
            });
            if (response.ok) {
                accountButton.disabled = true;
                submitButton.textContent = "Зарегистрироваться";
                currentStage = 'code-register'
                codeSection.style.display = 'flex'
                setTimeout(() => {
                    codeSection.classList.add('active')
                }, 10)
            } else {
                alert("Ошибка");
            }
        } else if (currentStage === 'login') {
            const password = document.getElementById('password').value;
            const response = await fetch('/api/auth/login/send-code', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email, password })
            });
            if (response.ok) {
                accountButton.disabled = true;
                submitButton.textContent = "Войти";
                currentStage = 'code-login'
                codeSection.style.display = 'flex'
                setTimeout(() => {
                    codeSection.classList.add('active')
                }, 10)
            } else {
                alert("Ошибка");
            }
        } else if (currentStage === 'code-register') {
            const password = document.getElementById('password').value;
            const nickname = document.getElementById('nickname').value;
            const code = document.getElementById('code').value;
            const response = await fetch('/api/auth/register', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email, password, nickname, code })
            });
            if (response.ok) {
                location.reload();
            } else {
                alert("Ошибка");
            }
        } else if (currentStage === 'code-login') {
            const password = document.getElementById('password').value;
            const code = document.getElementById('code').value;
            const response = await fetch('/api/auth/login', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email, password, code })
            });
            if (response.ok) {
                location.reload();
            } else {
                alert("Ошибка");
            }
        }


    });
})
