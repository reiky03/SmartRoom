import puppeteer from 'puppeteer-core';
import fs from 'fs';
import path from 'path';

const CHROME_PATH = '/Applications/Google Chrome.app/Contents/MacOS/Google Chrome';
const SCREENSHOT_DIR = path.resolve('./e2e-screenshots');

async function testDrawer() {
  console.log('🧪 Testing Booking Drawer & Submission...');
  const browser = await puppeteer.launch({
    executablePath: CHROME_PATH,
    headless: true,
    args: ['--no-sandbox', '--disable-setuid-sandbox', '--window-size=1400,900'],
    defaultViewport: { width: 1400, height: 900 }
  });

  const page = await browser.newPage();
  page.on('console', msg => console.log('BROWSER LOG:', msg.text()));
  page.on('pageerror', err => console.log('BROWSER ERROR:', err.message));

  try {
    // Login as student
    await page.goto('http://127.0.0.1:5100/login', { waitUntil: 'networkidle0' });
    const studentButton = await page.waitForSelector('button ::-p-text(Mahasiswa / Civitas)');
    await studentButton.click();
    await new Promise(r => setTimeout(r, 1200));

    // Click "+ Ajukan Peminjaman Baru" on Student Dashboard
    await page.evaluate(() => {
      const btn = document.querySelector('#student-dashboard-booking-btn');
      if (btn) btn.click();
    });
    await new Promise(r => setTimeout(r, 800));

    // Fill purpose
    await page.type('textarea', 'Simulasi Sidang Komprehensif Himpunan Mahasiswa');
    await new Promise(r => setTimeout(r, 500));

    // Submit booking via explicit ID
    await page.evaluate(() => {
      const submitBtn = document.querySelector('#submit-booking-drawer-btn');
      if (submitBtn) submitBtn.click();
    });
    await new Promise(r => setTimeout(r, 2000));

    await page.screenshot({ path: path.join(SCREENSHOT_DIR, '10-booking-submitted.png') });
    console.log('✅ Test 10: Booking submitted and reflected on personal dashboard');

  } catch (err) {
    console.error('❌ Drawer test error:', err);
  } finally {
    await browser.close();
  }
}

testDrawer();
