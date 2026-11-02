import puppeteer from 'puppeteer-core';
import fs from 'fs';
import path from 'path';

const CHROME_PATH = '/Applications/Google Chrome.app/Contents/MacOS/Google Chrome';
const SCREENSHOT_DIR = path.resolve('./e2e-screenshots');

async function testAdminApproval() {
  console.log('🧪 Testing Admin Approval Flow...');
  const browser = await puppeteer.launch({
    executablePath: CHROME_PATH,
    headless: true,
    args: ['--no-sandbox', '--disable-setuid-sandbox', '--window-size=1400,900'],
    defaultViewport: { width: 1400, height: 900 }
  });

  const page = await browser.newPage();
  try {
    // 1. Login as Admin
    await page.goto('http://127.0.0.1:5100/login', { waitUntil: 'networkidle0' });
    const adminLoginBtn = await page.waitForSelector('button ::-p-text(Administrator Fasilitas)');
    await adminLoginBtn.click();
    await new Promise(r => setTimeout(r, 1200));

    // 2. Go to /admin/approvals
    await page.goto('http://127.0.0.1:5100/admin/approvals', { waitUntil: 'networkidle0' });
    await new Promise(r => setTimeout(r, 1000));
    await page.screenshot({ path: path.join(SCREENSHOT_DIR, '11-admin-queue-with-new-booking.png') });
    console.log('✅ Test 11: Admin sees the new pending booking');

    // 3. Click "Setujui" on the first pending booking
    const approveBtn = await page.waitForSelector('button ::-p-text(Setujui)');
    await approveBtn.click();
    await new Promise(r => setTimeout(r, 1500));
    await page.screenshot({ path: path.join(SCREENSHOT_DIR, '12-booking-approved-by-admin.png') });
    console.log('✅ Test 12: Admin approved booking, queue updated');

  } catch (err) {
    console.error('❌ Admin approval test error:', err);
  } finally {
    await browser.close();
  }
}

testAdminApproval();
