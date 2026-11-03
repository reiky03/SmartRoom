import puppeteer from 'puppeteer-core';

const chromePath = process.env.CHROME_PATH || '/Applications/Google Chrome.app/Contents/MacOS/Google Chrome';
const baseUrl = process.env.SMARTROOM_BASE_URL || 'http://127.0.0.1:5101';

const browser = await puppeteer.launch({
  executablePath: chromePath,
  headless: true,
  args: ['--no-sandbox', '--disable-setuid-sandbox'],
});

const page = await browser.newPage();
const browserErrors = [];

page.on('pageerror', (error) => browserErrors.push(error.message));
page.on('console', (message) => {
  if (message.type() === 'error') browserErrors.push(message.text());
});

function assert(condition, message) {
  if (!condition) throw new Error(message);
}

async function assertNoOverflow(label) {
  const result = await page.evaluate(() => ({
    viewport: window.innerWidth,
    documentWidth: document.documentElement.scrollWidth,
  }));
  assert(result.documentWidth <= result.viewport, `${label} overflows horizontally: ${result.documentWidth}px > ${result.viewport}px`);
}

try {
  await page.setViewport({ width: 390, height: 844, deviceScaleFactor: 1, isMobile: true, hasTouch: true });
  await page.goto(`${baseUrl}/login`, { waitUntil: 'networkidle0' });
  await assertNoOverflow('Login mobile');

  await page.click('#quick-login-student-btn');
  await page.waitForFunction(() => location.pathname === '/student/dashboard');
  await page.waitForSelector('#student-dashboard-booking-btn', { visible: true });
  await assertNoOverflow('Student dashboard mobile');

  await page.click('#navbar-booking-btn');
  await page.waitForSelector('#booking-drawer-container', { visible: true });
  assert(await page.$('#submit-booking-drawer-btn'), 'Drawer submit action is missing');
  await page.waitForSelector('#booking-room-selector', { visible: true });
  await page.focus('#booking-room-selector');
  await page.keyboard.press('Enter');
  await page.waitForSelector('#booking-room-options button', { visible: true });
  await page.focus('#booking-room-options button');
  await page.keyboard.press('Enter');
  await page.waitForSelector('#booking-room-options', { hidden: true });
  await new Promise((resolve) => setTimeout(resolve, 450));
  await page.click('#close-booking-drawer-btn');
  await page.waitForSelector('#booking-drawer-container', { hidden: true });

  await new Promise((resolve) => setTimeout(resolve, 400));
  await page.click('#user-profile-btn');
  await page.waitForSelector('#switch-to-admin-btn', { visible: true });
  await page.click('#switch-to-admin-btn');
  await page.waitForFunction(() => location.pathname === '/admin/dashboard');

  await page.setViewport({ width: 1440, height: 900, deviceScaleFactor: 1 });
  const adminRoutes = ['/admin/dashboard', '/admin/approvals', '/admin/rooms', '/admin/bookings'];
  for (const route of adminRoutes) {
    await page.goto(`${baseUrl}${route}`, { waitUntil: 'networkidle0' });
    assert(new URL(page.url()).pathname === route, `Failed to load ${route}; current URL is ${page.url()}`);
    await assertNoOverflow(`${route} desktop`);
  }

  const nestedInteractiveCount = await page.evaluate(() => document.querySelectorAll('[role="button"] button').length);
  assert(nestedInteractiveCount === 0, `Found ${nestedInteractiveCount} nested interactive control(s)`);

  assert(browserErrors.length === 0, `Browser errors:\n${browserErrors.join('\n')}`);
  console.log('SmartRoom redesign smoke: PASS');
  console.log('Verified login, student access, keyboard room selection, drawer open/close, role switch, admin routes, responsive overflow, and interactive semantics.');
} finally {
  await browser.close();
}
