export class LogJsonInterop {
    static #looksLikeJson(str) {
        for (let i = 0; i < str.length; i++) {
            const c = str.charCodeAt(i);
            if (c <= 32) continue;
            return c === 123 || c === 91; // { or [
        }
        return false;
    }

    static logJson(input, group, level = "log") {
        let obj = input;

        if (typeof input === "string" && LogJsonInterop.#looksLikeJson(input)) {
            try { obj = JSON.parse(input); } catch { /* ignore */ }
        }

        const hasGroup = !!group;

        if (hasGroup) console.groupCollapsed(group);

        const fn = console[level] ?? console.log;
        fn.call(console, obj);

        if (hasGroup) console.groupEnd();
    }
}

window.LogJsonInterop = LogJsonInterop;