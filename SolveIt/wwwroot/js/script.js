window.QuillManager = (function () {
  const editors = new Map();

  const toolbarOptions = [
    ["bold", "italic", "underline", "strike"],
    ["blockquote", "code-block"],
    [{ header: 1 }, { header: 2 }],
    [{ list: "ordered" }, { list: "bullet" }],
    ["link", "image"],
    ["clean"],
  ];

  function init(id) {
    const element = document.getElementById(id);
    if (!element) return;

    if (editors.has(id)) return;

    const quill = new Quill(element, {
      modules: {
        toolbar: {
          container: toolbarOptions,
          handlers: {
            image: function () {
              selectAndUploadImage(quill);
            },
          },
        },
      },
      theme: "snow",
      placeholder: "Describe your technical hurdle...",
    });

    editors.set(id, quill);
  }

  function selectAndUploadImage(quill) {
    const input = document.createElement("input");
    input.type = "file";
    input.accept = "image/*";
    input.click();

    input.onchange = () => {
      const file = input.files[0];
      if (file) {
        uploadImageAndInsert(quill, file);
      }
    };
  }

  async function uploadImageAndInsert(quill, file) {
    try {
      const range = quill.getSelection(true) || { index: 0 };

      quill.insertText(range.index, "Uploading image...", { italic: true });

      const base64 = await fileToBase64(file);

      quill.deleteText(range.index, "Uploading image...".length);

      quill.insertEmbed(range.index, "image", base64);
    } catch (err) {
      console.error(err);
      alert("Image insert failed");
    }
  }

  function getContent(id, exclude = true) {
    const quill = editors.get(id);
    if (!quill) return "";

    let html = quill.root.innerHTML;
    if (exclude) {
      html = html.replace(/<img[^>]+src="data:image[^">]+"[^>]*>/g, "");
    }

    return html;
  }

  function setContent(id, html) {
    const quill = editors.get(id);
    if (quill) {
      quill.root.innerHTML = html;
    }
  }

  function destroy(id) {
    const element = document.getElementById(id);
    if (!element) return;

    const toolbar = element.previousElementSibling;
    if (toolbar && toolbar.classList.contains("ql-toolbar")) {
      toolbar.remove();
    }

    if (editors.has(id)) {
      editors.delete(id);
    }

    element.innerHTML = "";
    element.className = "rich-editor-container";
  }

  const uploadToCloud = async () => {
    const rawHtml = getContent("quill-desc", false);
    await loadAppwrite();
    const converted = await processHtmlImages(rawHtml);

    console.log("result converted  html", converted);
    return converted.html;
  };

  return {
    init,
    getContent,
    setContent,
    uploadToCloud,
    destroy,
  };
})();

const ed = () => document.getElementById("sol-editor");
let solOpen = false;

/**
 * Main functions
 * @param {string} rawHtml
 * @returns {Promise<{ html: string, files: File[] }>}
 */
async function processHtmlImages(rawHtml) {
  const { files, updatedHtml } = extractBase64Images(rawHtml);

  const uploadedUrls = [];

  for (const file of files) {
    const url = await uploadFn(file);
    uploadedUrls.push(url);
  }

  let finalHtml = updatedHtml;

  uploadedUrls.forEach((url, index) => {
    const placeholder = `__IMAGE_${index}__`;
    finalHtml = finalHtml.replace(placeholder, url);
  });

  return {
    html: finalHtml,
    files,
  };
}

function extractBase64Images(html) {
  const imgRegex = /<img[^>]+src="(data:image\/[^"]+)"[^>]*>/g;

  let match;
  let files = [];
  let index = 0;

  let updatedHtml = html;

  while ((match = imgRegex.exec(html)) !== null) {
    const base64 = match[1];

    const file = base64ToFile(base64, `image_${index}.png`);
    files.push(file);

    updatedHtml = updatedHtml.replace(base64, `__IMAGE_${index}__`);

    index++;
  }

  return { files, updatedHtml };
}

function base64ToFile(base64, filename) {
  const arr = base64.split(",");
  const mime = arr[0].match(/:(.*?);/)[1];
  const bstr = atob(arr[1]);

  let n = bstr.length;
  const u8arr = new Uint8Array(n);

  while (n--) {
    u8arr[n] = bstr.charCodeAt(n);
  }

  return new File([u8arr], filename, { type: mime });
}

function showToast(msg, err = false) {
  const t = document.getElementById("qv-toast");
  document.getElementById("qv-toast-msg").textContent = msg;
  t.className = "qv-toast qv-toast--show" + (err ? " qv-toast--error" : "");
  clearTimeout(t._t);
  t._t = setTimeout(() => (t.className = "qv-toast"), 3200);
}

function fileToBase64(file) {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();

    reader.onload = () => resolve(reader.result);
    reader.onerror = reject;

    reader.readAsDataURL(file);
  });
}

function openSolutions() {
  if (!solOpen) toggleSolutions();
}

function toggleSolutions() {
  solOpen = !solOpen;
  document.getElementById("solutions-body").style.display = solOpen
    ? "block"
    : "none";
  document.getElementById("reveal-label").textContent = solOpen
    ? "Hide Answers"
    : "Show Answers";
  document.getElementById("reveal-chevron").style.transform = solOpen
    ? "rotate(180deg)"
    : "";
}

async function handleVote(btn) {
  btn.disabled = true;
  const id = btn.dataset.id;
  try {
    const res = await fetch(`/api/questions/${id}/vote`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
    });
    if (!res.ok) throw new Error(await res.text());
    const data = await res.json(); // {voteCount}
    document.getElementById(`vote-count-${id}`).textContent = data.voteCount;
    btn.classList.add("qv-upvote--active");
    showToast("Upvoted!");
  } catch (e) {
    showToast(e.message || "Could not vote", true);
  } finally {
    btn.disabled = false;
  }
}

async function handleSolVote(btn) {
  btn.disabled = true;
  const id = btn.dataset.id,
    action = btn.dataset.action;
  try {
    const res = await fetch(`/api/solutions/${id}/${action}`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
    });
    if (!res.ok) throw new Error(await res.text());
    const data = await res.json(); // {likes, dislikes, votes}
    document.getElementById(`like-${id}`).textContent = data.likes;
    document.getElementById(`dislike-${id}`).textContent = data.dislikes;
    document.getElementById(`netvotes-${id}`).textContent = data.votes;
    showToast(action === "like" ? "👍 Liked!" : "👎 Disliked!");
  } catch (e) {
    showToast(e.message || "Could not vote", true);
  } finally {
    btn.disabled = false;
  }
}

async function submitSolution(questionId) {
  const editor = document.getElementById("sol-editor");
  const html = editor.innerHTML.trim();
  if (!html || !editor.textContent.trim()) {
    showToast("Answer cannot be empty", true);
    return;
  }

  const btn = document.getElementById("post-sol-btn");
  btn.disabled = true;
  btn.textContent = "Posting…";

  try {
    const res = await fetch(`/api/questions/${questionId}/solutions`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ body: html }),
    });
    if (!res.ok) throw new Error(await res.text());
    const sol = await res.json(); // {solId, body, userName, createdAt}

    const panel = document.getElementById("solutions-body");
    if (!solOpen) toggleSolutions();

    const empty = panel.querySelector(".qv-empty-state");
    if (empty) empty.remove();

    const card = document.createElement("article");
    card.className = "qv-sol-card qv-sol-card--new";
    card.id = `solution-${sol.solId}`;
    const avatarImg = sol.avatarUrl
      ? `<div class="qv-avatar qv-avatar--sm"><img src="${escapeHtml(sol.avatarUrl)}" alt="avatar"></div>`
      : `<div class="qv-avatar qv-avatar--sm">${escapeHtml((sol.userName || "?").charAt(0).toUpperCase())}</div>`;

    card.innerHTML = `
    <div class="qv-sol-vote-col">
        <button class="qv-sol-like" data-id="${sol.solId}" data-action="like" onclick="handleSolVote(this)">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 9V5a3 3 0 0 0-3-3l-4 9v11h11.28a2 2 0 0 0 2-1.7l1.38-9a2 2 0 0 0-2-2.3H14z" /><path d="M7 22H4a2 2 0 0 1-2-2v-7a2 2 0 0 1 2-2h3" /></svg>
            <span id="like-${sol.solId}">0</span>
        </button>
        <span class="qv-sol-netvotes" id="netvotes-${sol.solId}">0</span>
        <button class="qv-sol-dislike" data-id="${sol.solId}" data-action="dislike" onclick="handleSolVote(this)">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M10 15v4a3 3 0 0 0 3 3l4-9V2H5.72a2 2 0 0 0-2 1.7l-1.38 9a2 2 0 0 0 2 2.3H10z" /><path d="M17 2h2.67A2.31 2.31 0 0 1 22 4v7a2.31 2.31 0 0 1-2.33 2H17" /></svg>
            <span id="dislike-${sol.solId}">0</span>
        </button>
    </div>
    <div class="qv-sol-content">
        <div class="qv-sol-meta">
            ${avatarImg}
            <span class="qv-author-name">${escapeHtml(sol.userName)}</span>
            <span class="qv-author-date">just now</span>
        </div>
        <div class="qv-body-content">${sol.body}</div>
    </div>`;
    panel.prepend(card);

    const h = document.getElementById("sol-heading");
    const n = (parseInt(h.textContent) || 0) + 1;
    h.textContent = `${n} Answer${n !== 1 ? "s" : ""}`;

    editor.innerHTML = "";
    showToast("Answer posted!");
  } catch (e) {
    showToast(e.message || "Failed to post", true);
  } finally {
    btn.disabled = false;
    btn.innerHTML = `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="22" y1="2" x2="11" y2="13" /><polygon points="22 2 15 22 11 13 2 9 22 2" /></svg> Post Answer`;
  }
}

async function submitComment(questionId) {
  const ta = document.getElementById("comment-input");
  const body = ta.value.trim();
  if (!body) {
    showToast("Comment cannot be empty", true);
    return;
  }

  const btn = document.querySelector(".qv-comment-submit");
  btn.disabled = true;
  btn.textContent = "Posting…";

  try {
    const res = await fetch(`/api/questions/${questionId}/comments`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ body }),
    });
    if (!res.ok) throw new Error(await res.text());
    const c = await res.json(); // {commentId, body, userName}

    document.getElementById("no-comments-msg")?.remove();

    const div = document.createElement("div");
    div.className = "qv-comment qv-comment--new";
    div.id = `comment-${c.commentId}`;
    div.innerHTML = `
    <div class="qv-avatar qv-avatar--xs">${escapeHtml(c.userName.charAt(0).toUpperCase())}</div>
    <div class="qv-comment-bubble">
        <div class="qv-comment-header">
            <span class="qv-comment-author">${escapeHtml(c.userName)}</span>
            <span class="qv-comment-time">just now</span>
        </div>
        <p class="qv-comment-text">${escapeHtml(c.body)}</p>
    </div>`;
    document.getElementById("comment-list").prepend(div);

    const badge = document.getElementById("comment-count-badge");
    const side = document.getElementById("sidebar-comment-count");
    const n = (parseInt(badge.textContent) || 0) + 1;
    badge.textContent = n;
    if (side) side.textContent = n;

    ta.value = "";
    document.getElementById("char-count").textContent = "0 / 1000";
    showToast("Comment posted!");
  } catch (e) {
    showToast(e.message || "Failed to post", true);
  } finally {
    btn.disabled = false;
    btn.textContent = "Comment";
  }
}

function fmtSol(cmd) {
  ed().focus();
  document.execCommand(cmd, false, null);
}

function fmtHeading() {
  ed().focus();
  document.execCommand("formatBlock", false, "h3");
}

function fmtBlockquote() {
  ed().focus();
  document.execCommand("formatBlock", false, "blockquote");
}

function fmtCode() {
  ed().focus();
  const sel = window.getSelection();
  if (!sel.rangeCount) return;
  const r = sel.getRangeAt(0);
  const code = document.createElement("code");
  code.textContent = sel.toString() || "code";
  r.deleteContents();
  r.insertNode(code);
}

function fmtLink() {
  const url = prompt("URL:");
  if (url) {
    ed().focus();
    document.execCommand("createLink", false, url);
  }
}

function updateCharCount(el) {
  document.getElementById("char-count").textContent =
    `${el.value.length} / 1000`;
}

function escapeHtml(s) {
  return String(s)
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;")
    .replace(/'/g, "&#039;");
}
