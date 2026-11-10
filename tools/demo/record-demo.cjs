const { chromium } = require('playwright');
const { mkdir, readdir, rename } = require('node:fs/promises');
const path = require('node:path');

const baseUrl = process.env.BOOKSHELF_BASE_URL || 'http://127.0.0.1:5097';
const outputDirectory = path.resolve(__dirname, '../../docs/video');

(async () => {
  await mkdir(outputDirectory, { recursive: true });
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({
    viewport: { width: 1440, height: 900 },
    recordVideo: { dir: outputDirectory, size: { width: 1440, height: 900 } },
  });
  const page = await context.newPage();
  const pause = milliseconds => page.waitForTimeout(milliseconds);
  const caption = text => page.locator('#demo-caption').evaluate((element, value) => { element.textContent = value; }, text);

  await page.goto(baseUrl);
  await page.locator('.book-card').first().waitFor();
  await page.evaluate(() => {
    const banner = document.createElement('div');
    banner.textContent = 'DEMONSTRAÇÃO · ASP.NET CORE + SQLITE';
    banner.style.cssText = 'position:fixed;right:18px;top:94px;z-index:30;background:#e4f5ed;color:#17644e;border:1px solid #9fceb9;border-radius:8px;padding:8px 12px;font:700 11px DM Sans,sans-serif;letter-spacing:.4px';
    document.body.append(banner);
    const caption = document.createElement('div');
    caption.id = 'demo-caption';
    caption.style.cssText = 'position:fixed;left:50%;bottom:24px;transform:translateX(-50%);z-index:30;background:rgba(23,58,54,.95);color:white;border-radius:10px;padding:13px 22px;font:600 16px DM Sans,sans-serif;box-shadow:0 8px 28px rgba(0,0,0,.16)';
    document.body.append(caption);
  });

  await caption('BookShelf · biblioteca conectada à API e ao banco SQLite'); await pause(2200);
  await page.getByRole('button', { name: 'Lendo', exact: true }).click(); await pause(1100);
  await caption('Filtre os livros em andamento'); await pause(1100);
  await page.getByRole('button', { name: 'Todos', exact: true }).click(); await pause(800);
  await page.getByRole('button', { name: /Adicionar livro/ }).click(); await pause(700);
  await caption('Cadastre um livro pela interface web'); await pause(800);
  await page.locator('[name=title]').fill('Software Architecture: The Hard Parts');
  await page.locator('[name=author]').fill('Neal Ford, Mark Richards et al.');
  await page.locator('[name=genre]').fill('Architecture');
  await page.locator('[name=totalPages]').fill('400');
  await page.locator('[name=pagesRead]').fill('0');
  await page.locator('[name=status]').selectOption('WantToRead');
  await page.getByRole('button', { name: 'Salvar livro' }).click(); await pause(1400);
  const card = page.locator('.book-card').filter({ hasText: 'Software Architecture: The Hard Parts' });
  await card.waitFor();
  await caption('O cadastro foi persistido e retornou na listagem da API'); await pause(1100);
  await card.locator('[data-edit]').click(); await pause(600);
  await page.locator('[name=status]').selectOption('Reading');
  await page.locator('[name=pagesRead]').fill('125');
  await page.getByRole('button', { name: 'Salvar livro' }).click(); await pause(1400);
  await caption('Atualize o progresso; a API recalcula a projeção de leitura'); await pause(1400);
  page.once('dialog', dialog => dialog.accept());
  await card.locator('[data-delete]').click(); await pause(1300);
  await caption('A exclusão é refletida na biblioteca e nos indicadores'); await pause(1700);

  await context.close();
  await browser.close();
  const videos = (await readdir(outputDirectory)).filter(name => name.endsWith('.webm') && name !== 'BookShelf-demo.webm');
  if (videos.length !== 1) throw new Error(`Expected exactly one recorded video; found ${videos.length}`);
  const generatedPath = path.join(outputDirectory, videos[0]);
  const finalPath = path.join(outputDirectory, 'BookShelf-demo.webm');
  if (generatedPath !== finalPath) await rename(generatedPath, finalPath);
  console.log(`Recorded integrated demo: ${finalPath}`);
})().catch(error => { console.error(error); process.exit(1); });
