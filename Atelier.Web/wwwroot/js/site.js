(() => {
  const header = document.querySelector('[data-header]');
  const nav = document.querySelector('.primary-nav');
  const toggle = document.querySelector('.nav-toggle');
  const reduced = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

  document.querySelectorAll('.hero-card, .content-card, .card-soft, .blog-card, .project-card').forEach((element, index) => {
    element.classList.add('reveal');
    element.style.transitionDelay = `${Math.min(index % 4, 3) * 70}ms`;
  });

  window.addEventListener('scroll', () => header?.classList.toggle('is-scrolled', scrollY > 12), { passive: true });
  toggle?.addEventListener('click', () => {
    const open = nav?.classList.toggle('is-open');
    toggle.setAttribute('aria-expanded', String(Boolean(open)));
  });

  if (reduced) return document.querySelectorAll('.reveal').forEach(el => el.classList.add('is-visible'));
  const observer = new IntersectionObserver(entries => entries.forEach(entry => {
    if (entry.isIntersecting) { entry.target.classList.add('is-visible'); observer.unobserve(entry.target); }
  }), { threshold: .12, rootMargin: '0px 0px -40px' });
  document.querySelectorAll('.reveal').forEach(el => observer.observe(el));

  document.querySelectorAll('form').forEach(form => form.addEventListener('submit', () => {
    const button = form.querySelector('button[type="submit"]');
    if (!button || !form.checkValidity()) return;
    button.disabled = true;
    button.dataset.originalText = button.textContent;
    button.textContent = 'در حال ارسال…';
  }));

  const visual = document.querySelector('[data-parallax]');
  window.addEventListener('pointermove', event => {
    if (!visual || innerWidth < 992) return;
    const x = (event.clientX / innerWidth - .5) * 10;
    const y = (event.clientY / innerHeight - .5) * 10;
    visual.style.transform = `translate3d(${x}px, ${y}px, 0)`;
  }, { passive: true });
})();
