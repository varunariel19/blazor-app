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

  const uploadToCloud = async (editorName) => {
    const rawHtml = getContent(editorName, false);
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

window.scrollToBottom = (el) => { if (el) el.scrollTop = el.scrollHeight; }


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

async function handleVote(quesId,btnElement) {
  
    try {
        
        btnElement.disabled = true;

        const res = await fetch(`/api/questions/${quesId}/vote`, {
            method: "POST",
            headers: { "Content-Type": "application/json" }
        });

        if (!res.ok) {
            const errorText = await res.text();
            throw new Error(errorText);
        }

        const data = await res.json();

        const voteEl = document.getElementById(`vote-count-${quesId}`);
        if (voteEl) {
            voteEl.textContent = data.voteCount;
        }

        btnElement.classList.add("qv-upvote--active");

        showToast(data.message);
    } catch (e) {
        showToast(e.message || "Could not vote", true);
    } finally {
        btnElement.disabled = false;
    }
}


async function handleSolVote(btnElement, solId, quesId) {
    try {
        btnElement.disabled = true;

        const res = await fetch(`/api/solutions/${quesId}/${solId}/vote`, {
            method: "POST",
            headers: { "Content-Type": "application/json" }
        });

        if (!res.ok) {
            const errorText = await res.text();
            throw new Error(errorText);
        }

        const data = await res.json();

        const voteEl = document.getElementById(`sol-vote-count-${solId}`);
        if (voteEl) {
            voteEl.textContent = `${data.voteCount}`;
        }

        btnElement.classList.add("qv-upvote--active");

        showToast(data.message || "Vote recorded!");

    } catch (e) {
        console.error("Voting failed:", e);
        showToast(e.message || "Could not vote", true);
    } finally {
        btnElement.disabled = false;
    }
}

async function submitSolution(questionId) {
    const html = await window.QuillManager.uploadToCloud("sol-editor");
    const editor = document.getElementById("sol-editor");

    const textOnly = html.replace(/<[^>]*>/g, "").trim();

    if (!textOnly.length) {
        showToast("Answer cannot be empty", true);
        return;
    }

    const btn = document.getElementById("post-sol-btn");
    btn.disabled = true;
    const originalContent = btn.innerHTML;
    btn.textContent = "Posting…";

    try {
        const res = await fetch(`/api/questions/${questionId}/solutions`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ body: html }),
        });

        if (!res.ok) throw new Error(await res.text());
        const sol = await res.json();

        const panel = document.getElementById("solutions-body");

        if (typeof solOpen !== 'undefined' && !solOpen) toggleSolutions();
        else panel.style.display = "block";

        const empty = panel.querySelector(".qv-empty-state");
        if (empty) empty.remove();

        const card = document.createElement("article");
        card.className = "qv-sol-card qv-sol-card--new";
        card.id = `solution-${sol.solId}`;

        const avatarHtml = sol.avatarUrl
            ? `<div class="qv-avatar qv-avatar--sm"><img src="${sol.avatarUrl}" alt=""></div>`
            : `<div class="qv-avatar qv-avatar--sm" style="--av-color: #3b82f6">${(sol.userName || "?").charAt(0).toUpperCase()}</div>`;

        card.innerHTML = `
            <div class="qv-sol-vote-col">
                <button class="up-vote-sol" 
                        onclick="handleSolVote(this, '${sol.solId}', '${questionId}')">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3">
                        <polyline points="18 15 12 9 6 15" />
                    </svg>
                </button>
                <span class="sol-vote-count" id="sol-vote-count-${sol.solId}">0</span>
                <span class="sol-vote-label">votes</span>
            </div>
            <div class="qv-sol-content">
                <div class="qv-sol-meta">
                    ${avatarHtml}
                    <span class="qv-author-name">${sol.userName}</span>
                    <span class="qv-author-date">just now</span>
                </div>
                <div class="qv-body-content">${sol.body}</div>
            </div>`;

        panel.prepend(card);

        const h = document.getElementById("sol-heading");
        if (h) {
            const currentCount = parseInt(h.textContent) || 0;
            const newCount = currentCount + 1;
            h.textContent = `${newCount} Answer${newCount !== 1 ? "s" : ""}`;
        }

        if (window.quill) window.quill.setContents([]);
        else editor.innerHTML = "";

        showToast("Answer posted!");
    } catch (e) {
        showToast(e.message || "Failed to post", true);
    } finally {
        btn.disabled = false;
        btn.innerHTML = originalContent;
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
    const c = await res.json(); 

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
