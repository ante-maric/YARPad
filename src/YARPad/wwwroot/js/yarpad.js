window.sessionStore = {
    set: (key, value) => sessionStorage.setItem(key, value),
    get: (key) => sessionStorage.getItem(key),
    remove: (key) => sessionStorage.removeItem(key),
    clear: () => sessionStorage.clear(),
};

const themeCookieName = "yarpad_themestate";
const darkThemeClass = "yarpad-theme-dark";
const lightThemeClass = "yarpad-theme-light";

function readThemeCookie() {
    try {
        const match = document.cookie.match(
            new RegExp(`(?:^|; )${themeCookieName}=([^;]*)`)
        );
        if (!match || match.length < 2) return null;

        const decoded = decodeURIComponent(match[1]);
        const parsed = JSON.parse(decoded);
        if (parsed && typeof parsed.IsDarkMode === "boolean") {
            return parsed.IsDarkMode;
        }

        return null;
    } catch {
        return null;
    }
}

function writeThemeCookie(isDarkTheme) {
    try {
        const payload = encodeURIComponent(
            JSON.stringify({ IsDarkMode: !!isDarkTheme })
        );
        document.cookie = `${themeCookieName}=${payload}; path=/; max-age=31536000; samesite=lax`;
    } catch {
        // Ignore cookie write failures (e.g., disabled cookies)
    }
}

function applyThemeClass(isDarkTheme) {
    if (isDarkTheme) {
        document.body.classList.remove(lightThemeClass);
        document.body.classList.add(darkThemeClass);
    } else {
        document.body.classList.remove(darkThemeClass);
        document.body.classList.add(lightThemeClass);
    }
}

window.yarpadTheme = {
    applyFromCookie: function () {
        const isDarkTheme = readThemeCookie();
        if (typeof isDarkTheme === "boolean") {
            applyThemeClass(isDarkTheme);
        }
    },
    setTheme: function (isDarkTheme) {
        writeThemeCookie(isDarkTheme);
        applyThemeClass(isDarkTheme);
    },
};

window.yarpadFile = {
    openPicker: function (element) {
        if (element && typeof element.click === "function") {
            element.click();
        }
    },
    resetPicker: function (element) {
        if (element) {
            element.value = "";
        }
    },
    saveJson: function (fileName, content) {
        try {
            const blob = new Blob([content], { type: "application/json" });
            const url = URL.createObjectURL(blob);

            const anchor = document.createElement("a");
            anchor.href = url;
            anchor.download = fileName || "configuration.yrpd";
            anchor.style.display = "none";

            document.body.appendChild(anchor);
            anchor.click();
            document.body.removeChild(anchor);

            URL.revokeObjectURL(url);
        } catch (error) {
            console.error("Failed to save configuration file.", error);
        }
    },
};

function getSession(key) {
    try {
        return sessionStorage.getItem(key);
    } catch {
        return null;
    }
}

window.YarpadManageNavigation = {
    goBack: function () {
        const key = "yarpad_manage_origin";
        const target = getSession(key);

        // Optional: clear after use if you don't want re-use
        // sessionStorage.removeItem(key);

        if (target && typeof target === "string") {
            // Safer than replace in some UX cases. Use replace() if you don't want this page in history.
            window.location.assign(target);
        } else {
            const fallbackUrl = window.yarpadBasePath || "/";
            window.location.assign(fallbackUrl);
        }
    },
};
