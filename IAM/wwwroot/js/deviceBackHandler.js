const deviceBackHandler = () => {
    window.hasHistory = false;

    window.addEventListener("beforeunload", function (event) {
        window.hasHistory = true;
    });

    const handler = (e) => {
        history.back();
        setTimeout(function () {
            if (!window.hasHistory) {
                if (window.crey) {
                    window.crey.sendEventToAppUI(JSON.stringify({
                        event: "onApplicationExitConfirmDialog",
                        params: []
                    }));
                };
            }
        }, 200);
    }

    window.addEventListener("back_pressed", handler);
}
window.addEventListener("load", deviceBackHandler);