(function () {
    const modalElement = document.getElementById('mediaManagerModal');
    if (!modalElement) {
        return;
    }

    const modalBody = modalElement.querySelector('[data-media-modal-body]');
    const modal = new bootstrap.Modal(modalElement, { backdrop: 'static' });
    let activeOptions = null;

    async function loadPicker(selectedMediaId) {
        const url = new URL(modalElement.dataset.pickerUrl, window.location.origin);
        if (selectedMediaId) {
            url.searchParams.set('selectedMediaId', selectedMediaId);
        }
        const response = await fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
        modalBody.innerHTML = await response.text();
        bindPickerEvents();
    }

    function updateSelection(element) {
        if (!element) {
            return;
        }

        const altText = element.dataset.mediaAltText;
        if (!altText) {
            const warning = modalBody.querySelector('[data-media-warning]');
            if (warning) {
                warning.textContent = 'برای انتخاب تصویر باید متن جایگزین وارد کنید.';
                warning.classList.remove('d-none');
            }
            return;
        }

        const mediaId = element.dataset.mediaId;
        const mediaUrl = element.dataset.mediaUrl;

        const warning = modalBody.querySelector('[data-media-warning]');
        if (warning) {
            warning.textContent = '';
            warning.classList.add('d-none');
        }

        modalBody.querySelectorAll('[data-media-item]').forEach(item => item.classList.remove('selected'));
        element.classList.add('selected');

        if (activeOptions?.targetInputId) {
            const input = document.getElementById(activeOptions.targetInputId);
            if (input) {
                input.value = mediaId;
            }
        }

        if (activeOptions?.previewImageId) {
            const preview = document.getElementById(activeOptions.previewImageId);
            if (preview) {
                preview.src = mediaUrl;
                preview.classList.remove('d-none');
            }
        }

        if (activeOptions?.placeholderId) {
            const placeholder = document.getElementById(activeOptions.placeholderId);
            if (placeholder) {
                placeholder.classList.add('d-none');
            }
        }

        const status = modalBody.querySelector('[data-media-status]');
        if (status) {
            status.textContent = 'تصویر انتخاب شد';
        }

        if (activeOptions?.formId) {
            const form = document.getElementById(activeOptions.formId);
            if (form) {
                form.submit();
            }
        }

        modal.hide();
    }

    function createTile({ id, url, title, altText, srcSet }) {
        const isMissingAlt = !altText;
        const button = document.createElement('button');
        button.type = 'button';
        button.className = `media-tile ${isMissingAlt ? 'missing-alt' : ''}`;
        button.dataset.mediaItem = '';
        button.dataset.mediaId = id;
        button.dataset.mediaUrl = url;
        button.dataset.mediaTitle = title;
        button.dataset.mediaAltText = altText || '';
        button.innerHTML = `
            <span class="media-thumb">
                <img class="media-thumb-img" src="${url}" srcset="${srcSet || ''}" sizes="(max-width: 768px) 45vw, 220px" alt="${title}" loading="lazy" />
            </span>
            <span class="media-title">${title}</span>
            <span class="media-alt-text ${isMissingAlt ? 'text-danger' : 'text-muted'} small">
                ${isMissingAlt ? 'بدون متن جایگزین' : altText}
            </span>
            ${isMissingAlt ? `<span class="media-alt-action"><span class="btn btn-link btn-sm p-0 text-danger" role="button" tabindex="0" data-media-edit-alt data-media-id="${id}">ثبت Alt Text</span></span>` : ''}
        `;
        button.addEventListener('click', () => updateSelection(button));
        return button;
    }

    function bindPickerEvents() {
        const items = modalBody.querySelectorAll('[data-media-item]');
        items.forEach(item => {
            item.addEventListener('click', () => updateSelection(item));
        });

        const editButtons = modalBody.querySelectorAll('[data-media-edit-alt]');
        editButtons.forEach(button => {
            button.addEventListener('click', async (event) => {
                event.preventDefault();
                event.stopPropagation();
                const mediaId = button.dataset.mediaId;
                const newAltText = window.prompt('متن جایگزین تصویر را وارد کنید:');
                if (!newAltText || !newAltText.trim()) {
                    const warning = modalBody.querySelector('[data-media-warning]');
                    if (warning) {
                        warning.textContent = 'وارد کردن متن جایگزین تصویر (Alt Text) برای سئو الزامی است.';
                        warning.classList.remove('d-none');
                    }
                    return;
                }

                const tokenField = modalBody.querySelector('input[name="__RequestVerificationToken"]');
                const token = tokenField ? tokenField.value : '';
                const body = new URLSearchParams({
                    mediaId: mediaId,
                    altText: newAltText
                });
                if (token) {
                    body.append('__RequestVerificationToken', token);
                }

                const response = await fetch(modalElement.dataset.updateAltUrl, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded',
                        'X-Requested-With': 'XMLHttpRequest'
                    },
                    body: body
                });

                const warning = modalBody.querySelector('[data-media-warning]');
                if (!response.ok) {
                    const payload = await response.json();
                    if (warning) {
                        warning.textContent = payload.message || 'خطا در ذخیره متن جایگزین.';
                        warning.classList.remove('d-none');
                    }
                    return;
                }

                const payload = await response.json();
                const tile = modalBody.querySelector(`[data-media-item][data-media-id='${mediaId}']`);
                if (tile) {
                    tile.dataset.mediaAltText = payload.altText;
                    tile.classList.remove('missing-alt');
                    const altLabel = tile.querySelector('.media-alt-text');
                    if (altLabel) {
                        altLabel.textContent = payload.altText;
                        altLabel.classList.remove('text-danger');
                        altLabel.classList.add('text-muted');
                    }
                    const editWrapper = tile.querySelector('.media-alt-action');
                    if (editWrapper) {
                        editWrapper.remove();
                    }
                }

                if (warning) {
                    warning.textContent = '';
                    warning.classList.add('d-none');
                }
            });
        });

        const uploadForm = modalBody.querySelector('[data-media-upload-form]');
        if (uploadForm) {
            uploadForm.addEventListener('submit', async (event) => {
                event.preventDefault();
                const errorBox = modalBody.querySelector('[data-media-error]');
                if (errorBox) {
                    errorBox.textContent = '';
                }

                const formData = new FormData(uploadForm);

                const response = await fetch(modalElement.dataset.uploadUrl, {
                    method: 'POST',
                    body: formData,
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });

                if (!response.ok) {
                    const payload = await response.json();
                    if (errorBox) {
                        errorBox.textContent = payload.message || 'خطا در انجام عملیات.';
                    }
                    return;
                }

                const payload = await response.json();
                const grid = modalBody.querySelector('[data-media-grid]');
                if (grid) {
                    const tile = createTile({
                        id: payload.mediaId,
                        url: payload.sizedUrl || payload.thumbnailUrl,
                        title: payload.title,
                        altText: payload.altText,
                        srcSet: payload.srcSet
                    });
                    grid.prepend(tile);
                    updateSelection(tile);
                }
            });
        }

        const loadMoreButton = modalBody.querySelector('[data-media-load-more]');
        if (loadMoreButton) {
            loadMoreButton.addEventListener('click', async () => {
                const nextPage = parseInt(loadMoreButton.dataset.nextPage || '0', 10);
                if (!nextPage) {
                    return;
                }
                loadMoreButton.disabled = true;
                const listUrl = modalElement.dataset.listUrl;
                if (!listUrl) {
                    return;
                }
                const url = new URL(listUrl, window.location.origin);
                url.searchParams.set('page', nextPage);
                const response = await fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
                if (!response.ok) {
                    loadMoreButton.disabled = false;
                    return;
                }
                const payload = await response.json();
                const grid = modalBody.querySelector('[data-media-grid]');
                if (grid && payload.items) {
                    payload.items.forEach(item => {
                        const tile = createTile({
                            id: item.id,
                            url: item.sizedUrl || item.url,
                            title: item.title,
                            altText: item.altText,
                            srcSet: item.srcSet
                        });
                        grid.appendChild(tile);
                    });
                }

                if (payload.hasMore) {
                    loadMoreButton.dataset.nextPage = payload.nextPage;
                    loadMoreButton.disabled = false;
                } else {
                    loadMoreButton.classList.add('d-none');
                }
            });
        }
    }

    window.openMediaManager = function (options) {
        activeOptions = options || {};
        modal.show();
        loadPicker(activeOptions?.selectedMediaId);
    };
})();
