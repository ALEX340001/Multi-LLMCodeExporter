function buildTOC() {
    const content = document.querySelector('.page-content');
    if (!content) return;
    const headings = content.querySelectorAll('h2, h3');
    if (headings.length < 3) return;

    const toc = document.createElement('nav');
    toc.className = 'sticky-toc';
    toc.innerHTML = '<h4>Содержание</h4><ul></ul>';
    const list = toc.querySelector('ul');

    headings.forEach(h => {
        const li = document.createElement('li');
        const a = document.createElement('a');
        a.href = `#${h.id || h.textContent.toLowerCase().replace(/\s/g, '-')}`;
        a.textContent = h.textContent;
        if (h.tagName === 'H3') li.style.paddingLeft = '1.2rem';
        li.appendChild(a);
        list.appendChild(li);
    });

    // Вставляем в правую часть (например, после .page-content)
    const container = document.querySelector('.container');
    const wrapper = document.createElement('div');
    wrapper.className = 'content-with-toc';
    wrapper.style.display = 'flex';
    wrapper.style.gap = '2rem';
    wrapper.style.alignItems = 'flex-start';
    content.parentNode.insertBefore(wrapper, content);
    wrapper.appendChild(content);
    wrapper.appendChild(toc);

    // Стили для TOC
    const style = document.createElement('style');
    style.textContent = `
        .sticky-toc {
            position: sticky;
            top: 2rem;
            flex: 0 0 220px;
            background: var(--card-bg);
            border: 1px solid var(--card-border);
            border-radius: 12px;
            padding: 1rem;
            max-height: calc(100vh - 4rem);
            overflow-y: auto;
        }
        .sticky-toc h4 { margin: 0 0 0.5rem 0; font-size: 1rem; }
        .sticky-toc ul { list-style: none; padding: 0; margin: 0; }
        .sticky-toc li { margin-bottom: 0.4rem; }
        .sticky-toc a { color: var(--text-secondary); text-decoration: none; font-size: 0.9rem; }
        .sticky-toc a:hover { color: var(--link-color); }
        @media (max-width: 900px) { .sticky-toc { display: none; } }
    `;
    document.head.appendChild(style);
}