(() => {
  const markReady = () => {
    document.body.classList.add("admin-ready");
  };

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", markReady);
  } else {
    markReady();
  }

  document.querySelectorAll("form[data-admin-form]").forEach((form) => {
    form.addEventListener("submit", () => {
      if (form.dataset.submitted === "true") {
        return;
      }

      form.dataset.submitted = "true";

      const submitButtons = form.querySelectorAll("button[type='submit']");
      submitButtons.forEach((button) => {
        const loadingText = button.getAttribute("data-loading-text");
        if (loadingText) {
          button.dataset.originalText = button.innerHTML;
          button.innerHTML = `<span class="spinner-border spinner-border-sm ms-2" role="status" aria-hidden="true"></span>${loadingText}`;
        }
        button.disabled = true;
      });
    });
  });
})();
