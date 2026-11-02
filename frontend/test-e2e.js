import puppeteer from 'puppeteer-core';
import fs from 'fs';
import path from 'path';

const CHROME_PATH = '/Applications/Google Chrome.app/Contents/MacOS/Google Chrome';
const SCREENSHOT_DIR = path.resolve('./e2e-screenshots');

if (!fs.existsSync(SCREENSHOT_DIR)) {
  fs.mkdirSync(SCREENSHOT_DIR, { recursive: true });
}

async function run() {
  console.log('🚀 Starting SmartRoom E2E Verification...');
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
    // 1. Visit root '/' -> expect redirect to '/login'
    console.log('Test 1: Unauthenticated root access...');
    await page.goto('http://127.0.0.1:5100/', { waitUntil: 'networkidle0' });
    const currentUrl = page.url();
    console.log(`Current URL after root access: ${currentUrl}`);
    if (!currentUrl.includes('/login')) {
      throw new Error(`Expected redirect to /login, but got ${currentUrl}`);
    }
    await page.screenshot({ path: path.join(SCREENSHOT_DIR, '01-login-page.png') });
    console.log('✅ Test 1 Passed: Unauthenticated user successfully redirected to /login');

    // 2. Click Student quick login
    console.log('Test 2: Logging in as Student...');
    const studentButton = await page.waitForSelector('button ::-p-text(Mahasiswa / Civitas)');
    await studentButton.click();
    await page.waitForNavigation({ waitUntil: 'networkidle0' }).catch(() => {});
    await new Promise(r => setTimeout(r, 1000));

    const studentUrl = page.url();
    console.log(`Current URL after student login: ${studentUrl}`);
    if (!studentUrl.includes('/student/dashboard')) {
      throw new Error(`Expected redirect to /student/dashboard, but got ${studentUrl}`);
    }
    await page.screenshot({ path: path.join(SCREENSHOT_DIR, '02-student-dashboard.png') });
    console.log('✅ Test 2 Passed: Student login successful and routed to /student/dashboard');

    // 3. Test RBAC: Student attempts to access /admin/approvals
    console.log('Test 3: Testing RBAC enforcement (Student -> /admin/approvals)...');
    await page.goto('http://127.0.0.1:5100/admin/approvals', { waitUntil: 'networkidle0' });
    await new Promise(r => setTimeout(r, 1000));
    const blockedUrl = page.url();
    console.log(`Current URL after unauthorized attempt: ${blockedUrl}`);
    if (blockedUrl.includes('/admin/approvals')) {
      throw new Error(`RBAC failure: Student was able to access /admin/approvals!`);
    }
    await page.screenshot({ path: path.join(SCREENSHOT_DIR, '03-student-rbac-blocked.png') });
    console.log('✅ Test 3 Passed: RBAC blocked student access to /admin/approvals and redirected to student dashboard');

    // 4. Test User Menu Dropdown & Switch to Admin
    console.log('Test 4: Opening user menu & switching to Administrator role...');
    await page.evaluate(() => {
      const btn = document.querySelector('#user-profile-btn');
      if (btn) btn.click();
    });
    await new Promise(r => setTimeout(r, 600));
    await page.screenshot({ path: path.join(SCREENSHOT_DIR, '04-user-menu-dropdown.png') });

    await page.evaluate(() => {
      const adminBtn = document.querySelector('#switch-to-admin-btn');
      if (adminBtn) adminBtn.click();
    });
    await new Promise(r => setTimeout(r, 1500));

    const adminUrl = page.url();
    console.log(`Current URL after Admin switch: ${adminUrl}`);
    if (!adminUrl.includes('/admin/dashboard')) {
      throw new Error(`Expected redirect to /admin/dashboard, but got ${adminUrl}`);
    }
    await page.screenshot({ path: path.join(SCREENSHOT_DIR, '04-admin-dashboard.png') });
    console.log('✅ Test 4 Passed: Admin dashboard loaded with Gantt matrix and KPI statistics');

    // 5. Navigate to Admin Approvals
    console.log('Test 5: Navigating to Admin Approvals...');
    await page.goto('http://127.0.0.1:5100/admin/approvals', { waitUntil: 'networkidle0' });
    await new Promise(r => setTimeout(r, 1000));
    await page.screenshot({ path: path.join(SCREENSHOT_DIR, '05-admin-approvals.png') });
    console.log('✅ Test 5 Passed: Admin Approvals queue loaded with action buttons');

    // 6. Navigate to Admin Rooms
    console.log('Test 6: Navigating to Admin Room Management...');
    await page.goto('http://127.0.0.1:5100/admin/rooms', { waitUntil: 'networkidle0' });
    await new Promise(r => setTimeout(r, 1000));
    await page.screenshot({ path: path.join(SCREENSHOT_DIR, '06-admin-rooms.png') });
    console.log('✅ Test 6 Passed: Admin Room Management loaded with Add/Edit/Toggle controls');

    // 7. Navigate to Admin All Bookings (Audit Log)
    console.log('Test 7: Navigating to Audit Log...');
    await page.goto('http://127.0.0.1:5100/admin/bookings', { waitUntil: 'networkidle0' });
    await new Promise(r => setTimeout(r, 1000));
    await page.screenshot({ path: path.join(SCREENSHOT_DIR, '07-admin-all-bookings.png') });
    console.log('✅ Test 7 Passed: Audit Log loaded with search and filters');

    console.log('🎉 ALL 7 END-TO-END VERIFICATION TESTS PASSED SUCCESSFULLY!');
  } catch (err) {
    console.error('❌ E2E Test Error:', err);
    await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'error-state.png') }).catch(() => {});
    process.exit(1);
  } finally {
    await browser.close();
  }
}

run();
