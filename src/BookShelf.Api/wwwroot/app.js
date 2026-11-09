const api = '/api/books';
const statusLabel = { Reading: 'Lendo', Completed: 'Concluido', WantToRead: 'Quero ler', Paused: 'Pausado' };
const state = { search: '', status: '', books: [] };
const booksEl = document.querySelector('#books');
const stateEl = document.querySelector('#state');
const dialog = document.querySelector('#book-dialog');
const form = document.querySelector('#book-form');

async function request(url, options) {
  const response = await fetch(url, { headers: { 'Content-Type': 'application/json' }, ...options });
  if (response.status === 204) return null;
  const data = await response.json();
  if (!response.ok) throw data;
  return data;
}

async function load() {
  showState('Carregando sua biblioteca...');
  try {
    const query = new URLSearchParams();
    if (state.search) query.set('search', state.search);
    if (state.status) query.set('status', state.status);
    const [result, summary] = await Promise.all([request(`${api}?${query}`), request(`${api}/summary`)]);
    state.books = result.items;
    renderBooks(); renderSummary(summary);
    document.querySelector('#result-count').textContent = `${result.total} ${result.total === 1 ? 'livro' : 'livros'}`;
  } catch { showState('Nao foi possivel carregar os livros.', true); }
}

function renderSummary(summary) {
  document.querySelector('#stat-total').textContent = summary.total;
  document.querySelector('#stat-reading').textContent = summary.reading;
  document.querySelector('#stat-completed').textContent = summary.completed;
  document.querySelector('#stat-pages').textContent = summary.pagesRead.toLocaleString('pt-BR');
}

function renderBooks() {
  stateEl.hidden = true;
  if (!state.books.length) { booksEl.innerHTML = ''; showState('<strong>Nenhum livro encontrado</strong>Ajuste os filtros ou adicione uma nova leitura.'); return; }
  booksEl.innerHTML = state.books.map(book => `
    <article class="book-card">
      <div class="book-meta"><span class="status ${book.status}">${statusLabel[book.status]}</span><span>${book.genre}</span></div>
      <h3>${escapeHtml(book.title)}</h3><p class="author">${escapeHtml(book.author)}</p>
      <div class="progress-row"><span>Progresso</span><strong>${book.projection.progressPercent}%</strong></div>
      <div class="progress"><i style="width:${book.projection.progressPercent}%"></i></div>
      <div class="card-footer"><span>${book.pagesRead} de ${book.totalPages} paginas</span>
        <div class="card-actions"><button data-edit="${book.id}" aria-label="Editar ${escapeHtml(book.title)}">&#9998;</button><button class="danger" data-delete="${book.id}" aria-label="Excluir ${escapeHtml(book.title)}">&#128465;</button></div>
      </div>
    </article>`).join('');
}

function showState(message) { stateEl.innerHTML = message; stateEl.hidden = false; booksEl.innerHTML = ''; }
function toast(message) { const el = document.querySelector('#toast'); el.textContent = message; el.classList.add('show'); setTimeout(() => el.classList.remove('show'), 2500); }
function escapeHtml(value) { const div = document.createElement('div'); div.textContent = value; return div.innerHTML; }

function openModal(book) {
  form.reset(); document.querySelector('#form-error').hidden = true;
  document.querySelector('#book-id').value = book?.id || '';
  document.querySelector('#modal-title').textContent = book ? 'Editar livro' : 'Adicionar livro';
  if (book) for (const field of ['title','author','genre','isbn','status','totalPages','pagesRead','rating','notes']) form.elements[field].value = book[field] ?? '';
  dialog.showModal();
}

document.querySelectorAll('[data-open-modal]').forEach(button => button.addEventListener('click', () => openModal()));
document.querySelectorAll('[data-close]').forEach(button => button.addEventListener('click', () => dialog.close()));
document.querySelector('#search').addEventListener('input', debounce(event => { state.search = event.target.value; load(); }, 300));
document.querySelectorAll('.filter').forEach(button => button.addEventListener('click', () => {
  document.querySelectorAll('.filter').forEach(item => item.classList.remove('active')); button.classList.add('active'); state.status = button.dataset.status; load();
}));
booksEl.addEventListener('click', async event => {
  const edit = event.target.closest('[data-edit]'); const remove = event.target.closest('[data-delete]');
  if (edit) openModal(state.books.find(book => book.id === edit.dataset.edit));
  if (remove && confirm('Excluir este livro da biblioteca?')) { await request(`${api}/${remove.dataset.delete}`, { method: 'DELETE' }); toast('Livro removido.'); load(); }
});
form.addEventListener('submit', async event => {
  event.preventDefault(); const id = document.querySelector('#book-id').value; const values = Object.fromEntries(new FormData(form));
  const payload = { ...values, totalPages: Number(values.totalPages), pagesRead: Number(values.pagesRead), rating: values.rating ? Number(values.rating) : null };
  const error = document.querySelector('#form-error');
  try { await request(id ? `${api}/${id}` : api, { method: id ? 'PUT' : 'POST', body: JSON.stringify(payload) }); dialog.close(); toast(id ? 'Livro atualizado.' : 'Livro adicionado.'); load(); }
  catch (problem) { error.textContent = problem.errors ? Object.values(problem.errors).flat().join(' ') : problem.title || 'Nao foi possivel salvar.'; error.hidden = false; }
});
function debounce(fn, delay) { let timer; return (...args) => { clearTimeout(timer); timer = setTimeout(() => fn(...args), delay); }; }
load();
