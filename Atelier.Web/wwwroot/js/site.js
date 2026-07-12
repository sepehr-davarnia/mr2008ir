(() => {
  const revealElements = document.querySelectorAll(".reveal");
  if (revealElements.length === 0) {
    return;
  }

  if (!("IntersectionObserver" in window)) {
    revealElements.forEach((element) => element.classList.add("is-visible"));
    return;
  }

  const observer = new IntersectionObserver(
    (entries) => {
      entries.forEach((entry) => {
        if (entry.isIntersecting) {
          entry.target.classList.add("is-visible");
          observer.unobserve(entry.target);
        }
      });
    },
    { threshold: 0.15 }
  );

  revealElements.forEach((element) => observer.observe(element));
})();

(() => {
  const header = document.querySelector(".site-header");
  if (!header) {
    return;
  }

  const toggleStickyState = () => {
    if (window.scrollY > 12) {
      header.classList.add("is-sticky");
    } else {
      header.classList.remove("is-sticky");
    }
  };

  toggleStickyState();
  window.addEventListener("scroll", toggleStickyState, { passive: true });
})();
