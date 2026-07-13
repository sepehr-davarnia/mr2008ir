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
  document.addEventListener('keydown', event => {
    if (event.key !== 'Escape' || !nav?.classList.contains('is-open')) return;
    nav.classList.remove('is-open');
    toggle?.setAttribute('aria-expanded', 'false');
    toggle?.focus();
  });

  const toast = document.querySelector('[data-cart-toast]');
  const announceCart = message => {
    if (!toast) return;
    toast.textContent = message;
    toast.classList.add('is-visible');
    window.setTimeout(() => toast.classList.remove('is-visible'), 3200);
  };
  document.querySelectorAll('[data-cart-add]').forEach(form => form.addEventListener('submit', async event => {
    event.preventDefault();
    event.stopImmediatePropagation();
    const button = form.querySelector('button[type="submit"]');
    if (button) { button.disabled = true; button.classList.add('is-loading'); }
    try {
      const response = await fetch(form.action, { method: 'POST', body: new FormData(form), headers: { Accept: 'application/json' } });
      if (!response.ok) throw new Error('cart');
      const result = await response.json();
      const count = document.querySelector('[data-cart-count]');
      if (count) { count.textContent = result.count; count.hidden = false; count.classList.remove('d-none'); }
      announceCart(result.message);
      button?.classList.add('is-added');
      const label = button?.querySelector('span');
      if (label) label.textContent = button.classList.contains('quick-cart') ? '✓' : 'به سبد اضافه شد ✓';
    } catch {
      form.submit();
    } finally {
      if (button) { button.disabled = false; button.classList.remove('is-loading'); }
    }
  }));

  document.querySelector('[data-copy]')?.addEventListener('click', async event => {
    const button = event.currentTarget;
    await navigator.clipboard?.writeText(button.dataset.copy);
    button.textContent = 'کپی شد ✓';
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
