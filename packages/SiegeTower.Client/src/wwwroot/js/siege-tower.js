window.siegeTower = {
    submitGithubAccessToken: function (appId, installationId, privateKey) {
        const windowName = "siegetower-github-access-token";
        const resultWindow = window.open("about:blank", windowName);
        if (!resultWindow) {
            return;
        }

        const form = document.createElement("form");
        form.method = "post";
        form.action = "/api/github-access-token/browser";
        form.target = windowName;

        for (const [name, value] of Object.entries({ appId, installationId, privateKey })) {
            const input = document.createElement("input");
            input.type = "hidden";
            input.name = name;
            input.value = value;
            form.appendChild(input);
        }

        document.body.appendChild(form);
        form.submit();
        form.remove();
    }
};