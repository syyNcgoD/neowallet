import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '30s', target: 20 },
    { duration: '1m', target: 50 },
    { duration: '30s', target: 0 },
  ],
  thresholds: {
    http_req_duration: ['p(95)<300'], // 95% of requests must complete below 300ms
    http_req_failed: ['rate<0.01'],    // less than 1% failure rate
  },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';

export default function () {
  const email = `user_${__VU}_${__ITER}_${Date.now()}@domain.com`;
  const password = 'Password@123';

  // 1. Health check
  const healthRes = http.get(`${BASE_URL}/`);
  check(healthRes, {
    'health check status is 200': (r) => r.status === 200,
  });

  // 2. Register
  const registerPayload = JSON.stringify({
    email: email,
    password: password,
    role: 1,
  });

  const registerRes = http.post(`${BASE_URL}/api/auth/register`, registerPayload, {
    headers: { 'Content-Type': 'application/json' },
  });

  check(registerRes, {
    'register status is 201': (r) => r.status === 201,
  });

  let token = '';
  let userId = '';
  if (registerRes.status === 201) {
    const authData = registerRes.json();
    token = authData.accessToken;
    userId = authData.user.id;
  }

  // 3. Create Wallet
  if (userId) {
    const walletPayload = JSON.stringify({
      ownerId: userId,
      currency: 'USD',
    });

    const walletRes = http.post(`${BASE_URL}/api/wallets`, walletPayload, {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`,
      },
    });

    check(walletRes, {
      'create wallet status is 201': (r) => r.status === 201,
    });

    let walletId = '';
    if (walletRes.status === 201) {
      walletId = walletRes.json().id;
    }

    // 4. Deposit
    if (walletId) {
      const depositPayload = JSON.stringify({
        amount: 500,
        currency: 'USD',
        reference: `DEP-${Date.now()}`,
        description: 'Load test deposit',
      });

      const depositRes = http.post(`${BASE_URL}/api/wallets/${walletId}/deposit`, depositPayload, {
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`,
        },
      });

      check(depositRes, {
        'deposit status is 200': (r) => r.status === 200,
      });

      // 5. Get Summary
      const summaryRes = http.get(`${BASE_URL}/api/wallets/${walletId}/summary`, {
        headers: {
          'Authorization': `Bearer ${token}`,
        },
      });

      check(summaryRes, {
        'summary status is 200': (r) => r.status === 200,
        'balance matches deposit': (r) => r.json().balance >= 500,
      });
    }
  }

  sleep(1);
}
