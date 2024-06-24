export class JsonLogger {
    static logJson(jsonString, group, level = 'log') {
        let obj;

        if (jsonString) {
            try {
                obj = JSON.parse(jsonString);
            } catch (e) {
                obj = jsonString; // Use the original string if parsing fails
            }
        }

        console.group(group);

        if (obj) {
            switch (level) {
                case "trace":
                    console.trace(obj);
                    break;
                case "info":
                    console.info(obj);
                    break;
                case "warn":
                    console.warn(obj);
                    break;
                case "error":
                    console.error(obj);
                    break;
                case "debug":
                    console.debug(obj);
                    break;
                case "log":
                default:
                    console.log(obj);
                    break;
            }
        }

        console.groupEnd();
    }
}

window.JsonLogger = new JsonLogger();