// ASP.NET Core (.NET 8, cross-platform / Linux) minimal web app that serves a
// browser-based Pong game. The game runs client-side (HTML5 canvas + JS); this
// server just serves the page and a health-check endpoint.

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

const string page = """
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>Pong on .NET (Linux)</title>
  <style>
    * { box-sizing: border-box; }
    body {
      margin: 0; min-height: 100vh;
      display: flex; flex-direction: column; align-items: center; justify-content: center;
      background: #7f1d1d; color: #e5e7eb;
      font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
    }
    h1 { font-weight: 600; margin: 0 0 0.25rem; letter-spacing: 0.02em; }
    p.sub { margin: 0 0 1rem; color: #94a3b8; font-size: 0.95rem; }
    canvas { background: #000; border: 2px solid #334155; border-radius: 8px; box-shadow: 0 12px 40px rgba(0,0,0,0.5); }
    .hint { margin-top: 0.9rem; color: #64748b; font-size: 0.9rem; }
    kbd { background:#1e293b; border:1px solid #334155; border-radius:4px; padding:0.1rem 0.4rem; font-size:0.85em; }
  </style>
</head>
<body>
  <h1>Pong</h1>
  <p class="sub">Running on AWS Elastic Beanstalk &middot; .NET (ASP.NET Core) on Linux</p>
  <canvas id="game" width="800" height="500" aria-label="Pong game"></canvas>
  <p class="hint">Move: <kbd>W</kbd>/<kbd>S</kbd> or <kbd>&uarr;</kbd>/<kbd>&darr;</kbd> &nbsp;&middot;&nbsp; <kbd>Space</kbd> to pause &middot; First to 7 wins</p>

  <script>
  (function () {
    const canvas = document.getElementById('game');
    const ctx = canvas.getContext('2d');
    const W = canvas.width, H = canvas.height;

    const PADDLE_W = 12, PADDLE_H = 90, PADDLE_MARGIN = 24;
    const BALL_R = 8, WIN_SCORE = 7;

    const player = { x: PADDLE_MARGIN, y: H / 2 - PADDLE_H / 2, score: 0, speed: 7 };
    const ai     = { x: W - PADDLE_MARGIN - PADDLE_W, y: H / 2 - PADDLE_H / 2, score: 0, speed: 5 };
    const ball   = { x: W / 2, y: H / 2, vx: 5, vy: 3, r: BALL_R };

    const keys = {};
    let paused = false, gameOver = false;

    document.addEventListener('keydown', function (e) {
      if (['ArrowUp', 'ArrowDown', ' '].includes(e.key)) e.preventDefault();
      if (e.key === ' ') { if (gameOver) resetGame(); else paused = !paused; }
      keys[e.key.toLowerCase()] = true;
      keys[e.key] = true;
    });
    document.addEventListener('keyup', function (e) { keys[e.key.toLowerCase()] = false; keys[e.key] = false; });

    function resetBall(dir) {
      ball.x = W / 2; ball.y = H / 2;
      ball.vx = 5 * (dir || (Math.random() < 0.5 ? 1 : -1));
      ball.vy = (Math.random() * 6 - 3);
    }
    function resetGame() {
      player.score = 0; ai.score = 0; gameOver = false; paused = false;
      player.y = ai.y = H / 2 - PADDLE_H / 2; resetBall();
    }

    function update() {
      if (paused || gameOver) return;

      if (keys['w'] || keys['arrowup'])   player.y -= player.speed;
      if (keys['s'] || keys['arrowdown']) player.y += player.speed;
      player.y = Math.max(0, Math.min(H - PADDLE_H, player.y));

      const target = ball.y - PADDLE_H / 2;
      if (ai.y < target - 4) ai.y += ai.speed;
      else if (ai.y > target + 4) ai.y -= ai.speed;
      ai.y = Math.max(0, Math.min(H - PADDLE_H, ai.y));

      ball.x += ball.vx; ball.y += ball.vy;

      if (ball.y - ball.r < 0)  { ball.y = ball.r; ball.vy *= -1; }
      if (ball.y + ball.r > H)  { ball.y = H - ball.r; ball.vy *= -1; }

      if (ball.x - ball.r < player.x + PADDLE_W &&
          ball.y > player.y && ball.y < player.y + PADDLE_H && ball.vx < 0) {
        ball.vx *= -1.05;
        ball.vy += ((ball.y - (player.y + PADDLE_H / 2)) / (PADDLE_H / 2)) * 3;
        ball.x = player.x + PADDLE_W + ball.r;
      }
      if (ball.x + ball.r > ai.x &&
          ball.y > ai.y && ball.y < ai.y + PADDLE_H && ball.vx > 0) {
        ball.vx *= -1.05;
        ball.vy += ((ball.y - (ai.y + PADDLE_H / 2)) / (PADDLE_H / 2)) * 3;
        ball.x = ai.x - ball.r;
      }

      if (ball.x < 0)  { ai.score++; check(); resetBall(1); }
      if (ball.x > W)  { player.score++; check(); resetBall(-1); }
    }

    function check() { if (player.score >= WIN_SCORE || ai.score >= WIN_SCORE) gameOver = true; }

    function draw() {
      ctx.fillStyle = '#000'; ctx.fillRect(0, 0, W, H);

      ctx.strokeStyle = '#1f2937'; ctx.setLineDash([10, 14]); ctx.lineWidth = 2;
      ctx.beginPath(); ctx.moveTo(W / 2, 0); ctx.lineTo(W / 2, H); ctx.stroke(); ctx.setLineDash([]);

      ctx.fillStyle = '#e5e7eb';
      ctx.fillRect(player.x, player.y, PADDLE_W, PADDLE_H);
      ctx.fillRect(ai.x, ai.y, PADDLE_W, PADDLE_H);

      ctx.beginPath(); ctx.arc(ball.x, ball.y, ball.r, 0, Math.PI * 2); ctx.fill();

      ctx.font = '42px monospace'; ctx.textAlign = 'center';
      ctx.fillText(player.score, W / 2 - 60, 56);
      ctx.fillText(ai.score, W / 2 + 60, 56);

      if (paused && !gameOver) {
        ctx.font = '28px monospace'; ctx.fillText('PAUSED', W / 2, H / 2);
      }
      if (gameOver) {
        ctx.font = '34px monospace';
        ctx.fillText(player.score > ai.score ? 'YOU WIN!' : 'YOU LOSE', W / 2, H / 2 - 10);
        ctx.font = '18px monospace'; ctx.fillStyle = '#94a3b8';
        ctx.fillText('Press Space to play again', W / 2, H / 2 + 28);
      }
    }

    function loop() { update(); draw(); requestAnimationFrame(loop); }
    resetBall(); loop();
  })();
  </script>
</body>
</html>
""";

app.MapGet("/", () => Results.Content(page, "text/html"));
app.MapGet("/health", () => Results.Text("OK"));

// Listen on the platform-provided PORT if set; default 5000 (EB .NET Core on
// Linux convention). For Modern Apps/buildpacks set PORT (or service-port) to match.
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Run($"http://0.0.0.0:{port}");
