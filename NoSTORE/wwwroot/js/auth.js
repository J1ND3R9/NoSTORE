const token = localStorage.getItem("token");

async function apiFetch(url, options = {}) {
    if (!token) {
        return;
    }
    const headers = {
        ...(token && { Authorization: 'Bearer ${token}' }),
        ...options.headers
    };
    const response = await fetch(url, {
        ...options,
        headers,
    });

    if (response.status === 401) {
        localStorage.removeItem("token");
        window.location.href = "/";
        return;
    }

    return response;
}

function parseJwt(token) {
    try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(atob(base64).split('').map(function (c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));
        return JSON.parse(jsonPayload);
    } catch (e) {
        console.error("Ошибка декодирования токена", e);
        return null;
    }
}

function headerChangeAuth() {
    const profile = document.getElementById('profile-text');
    const decodedToken = parseJwt(token);
    if (decodedToken) {
        const nickname = decodedToken.nickname;
        if (nickname) {
            profile.textContent = nickname;
        } else {
            console.warn("в JWT нет никнейма");
        }
    }
}

function getAuth() {
    if (!token) {
        console.warn("JWT не найден")
        return;
    }
    headerChangeAuth();
}

document.addEventListener('DOMContentLoaded', () => {
    getAuth();
});