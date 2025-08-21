export class LogJsonInterop {
    static #levels = Object.freeze({
        trace: console.trace.bind(console),
        info: console.info.bind(console),
        warn: console.warn.bind(console),
        error: console.error.bind(console),
        debug: console.debug.bind(console),
        log: console.log.bind(console)
    });

    static #looksLikeJson(str) {
        for (let i = 0; i < str.length; i++) {
            const c = str.charCodeAt(i);
            if (c <= 32)
                continue; // whitespace
            return c === 123 /* '{' */ || c === 91 /* '[' */;
        }
        return false;
    }

    static logJson(input, group, level = 'log') {
        let obj = input;

        if (typeof input === "string" && LogJsonInterop.#looksLikeJson(input)) {
            try {
                obj = JSON.parse(input);
            } catch {
                // fall back to original string
            }
        }

        if (group)
            console.group(group);

        const logger = LogJsonInterop.#levels[level] || LogJsonInterop.#levels.log;
        logger(obj);

        if (group)
            console.groupEnd();
    }
}

window.LogJsonInterop = LogJsonInterop;