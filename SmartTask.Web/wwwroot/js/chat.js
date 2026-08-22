/* ==========================================================
   SmartTask - Project Group Chat (with Reactions, Pin, Mentions)
   ========================================================== */

(function () {
    "use strict";

    const bootstrapEl = document.getElementById("chatBootstrap");
    if (!bootstrapEl) return;

    const state = JSON.parse(bootstrapEl.textContent);

    const EMOJI_LIST = ["👍", "❤️", "😂", "😮", "😢", "🙏", "🔥", "👏"];

    const app = {
        currentUserId: state.currentUserId,
        isJalali: state.isJalali,
        room: state.activeRoom || null,
        messages: [],
        online: new Set(),
        replyTo: null,
        editing: null,
        typingTimers: new Map(),
        typingSent: false,
        typingTimeout: null,
        loading: false,
        hasMore: false,
        mentionQuery: null,
        mentionStart: -1,
        mentionMembers: [],
        searchSkip: 0,
        searchHasMore: false,
        searchLoading: false
    };

    // ===== DOM =====
    const dom = {
        list: document.getElementById("chatList"),
        listSearch: document.getElementById("chatListSearch"),
        messages: document.getElementById("chatMessages"),
        body: document.getElementById("chatMessagesBody"),
        loadMore: document.getElementById("chatLoadMore"),
        loadMoreBtn: document.getElementById("chatLoadMoreBtn"),
        input: document.getElementById("chatInput"),
        sendBtn: document.getElementById("chatSendBtn"),
        attachBtn: document.getElementById("chatAttachBtn"),
        fileInput: document.getElementById("chatFileInput"),
        replyPreview: document.getElementById("chatReplyPreview"),
        replyName: document.getElementById("chatReplyName"),
        replyText: document.getElementById("chatReplyText"),
        replyCancel: document.getElementById("chatReplyCancel"),
        typing: document.getElementById("chatTyping"),
        roomName: document.getElementById("roomName"),
        roomAvatar: document.getElementById("roomAvatar"),
        roomStatus: document.getElementById("roomStatus"),
        memberCount: document.getElementById("roomMemberCount"),
        onlineCount: document.getElementById("roomOnlineCount"),
        membersPane: document.getElementById("chatMembersPane"),
        membersList: document.getElementById("chatMembersList"),
        membersToggle: document.getElementById("chatMembersToggle"),
        membersClose: document.getElementById("chatMembersClose"),
        testPushBtn: document.getElementById("chatTestPushBtn"),
        searchToggle: document.getElementById("chatSearchToggle"),
        searchBar: document.getElementById("chatSearchBar"),
        searchInput: document.getElementById("chatMessageSearch"),
        searchClose: document.getElementById("chatSearchClose"),
        banner: document.getElementById("chatConnectionBanner"),
        backBtn: document.getElementById("chatBackBtn"),
        appRoot: document.getElementById("chatApp"),
        uploadProgress: document.getElementById("chatUploadProgress"),
        uploadName: document.getElementById("chatUploadName"),
        progressFill: document.getElementById("chatProgressFill"),
        // Mention autocomplete
        mentionDropdown: null,
        // Emoji picker
        emojiPicker: null,
        // Pinned bar
        pinnedBar: document.getElementById("chatPinnedBar"),
        pinnedList: document.getElementById("chatPinnedList")
    };

    // ===== Helpers =====

    function esc(value) {
        if (value === null || value === undefined) return "";
        return String(value)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#39;");
    }

    function token() {
        const el = document.querySelector('#chatAntiForgeryForm input[name="__RequestVerificationToken"]');
        return el ? el.value : "";
    }

    const timeFormatter = new Intl.DateTimeFormat("fa-IR-u-nu-latn", {
        hour: "2-digit", minute: "2-digit", hour12: false
    });

    const dayFormatter = new Intl.DateTimeFormat(
        app.isJalali ? "fa-IR-u-ca-persian-nu-latn" : "fa-IR-u-ca-gregory-nu-latn",
        { year: "numeric", month: "long", day: "numeric" });

    function formatTime(iso) {
        if (!iso) return "";
        return timeFormatter.format(new Date(iso));
    }

    function dayKey(date) {
        return `${date.getFullYear()}-${date.getMonth()}-${date.getDate()}`;
    }

    function formatDay(iso) {
        const date = new Date(iso);
        const today = new Date();
        const yesterday = new Date();
        yesterday.setDate(today.getDate() - 1);

        if (dayKey(date) === dayKey(today)) return "امروز";
        if (dayKey(date) === dayKey(yesterday)) return "دیروز";
        return dayFormatter.format(date);
    }

    function formatListTime(iso) {
        if (!iso) return "";
        const date = new Date(iso);
        const today = new Date();
        if (dayKey(date) === dayKey(today)) return formatTime(iso);

        const yesterday = new Date();
        yesterday.setDate(today.getDate() - 1);
        if (dayKey(date) === dayKey(yesterday)) return "دیروز";

        return dayFormatter.format(date);
    }

    function formatLastSeen(iso) {
        if (!iso) return "آفلاین";

        const diffMinutes = Math.floor((Date.now() - new Date(iso).getTime()) / 60000);

        if (diffMinutes < 1) return "همین الان آنلاین بود";
        if (diffMinutes < 60) return `${diffMinutes} دقیقه پیش`;
        if (diffMinutes < 1440) return `${Math.floor(diffMinutes / 60)} ساعت پیش`;
        return `آخرین بازدید ${formatDay(iso)}`;
    }

    function formatSize(bytes) {
        if (!bytes) return "";
        if (bytes < 1024) return bytes + " بایت";
        if (bytes < 1048576) return (bytes / 1024).toFixed(1) + " کیلوبایت";
        return (bytes / 1048576).toFixed(1) + " مگابایت";
    }

    function initials(name) {
        if (!name) return "?";
        return name.trim().charAt(0);
    }

    function isNearBottom() {
        if (!dom.messages) return true;
        return dom.messages.scrollHeight - dom.messages.scrollTop - dom.messages.clientHeight < 150;
    }

    function scrollToBottom() {
        if (dom.messages) dom.messages.scrollTop = dom.messages.scrollHeight;
    }

    // ===== Rendering: messages =====

    function avatarMarkup(name, avatar, extraClass) {
        if (avatar) {
            return `<span class="chat-msg-avatar ${extraClass || ""}"><img src="${esc(avatar)}" alt="${esc(name)}" /></span>`;
        }
        return `<span class="chat-msg-avatar ${extraClass || ""}">${esc(initials(name))}</span>`;
    }

    function attachmentMarkup(message) {
        if (message.typeName === "Image") {
            return `
                <a class="chat-msg-image" href="${esc(message.attachmentPath)}" target="_blank" rel="noopener">
                    <img src="${esc(message.attachmentPath)}" alt="${esc(message.attachmentName)}" loading="lazy" />
                </a>`;
        }

        return `
            <a class="chat-msg-file" href="${esc(message.attachmentPath)}" target="_blank" rel="noopener" download>
                <span class="chat-file-icon"><i class="fa-solid fa-file-arrow-down"></i></span>
                <span class="chat-file-meta">
                    <b>${esc(message.attachmentName)}</b>
                    <small>${esc(formatSize(message.attachmentSize))}</small>
                </span>
            </a>`;
    }

    function replyMarkup(message) {
        if (!message.replyToMessageId) return "";

        if (!message.replyToSenderName) {
            return `<span class="chat-msg-reply chat-msg-reply-deleted"><i>پیام حذف شده</i></span>`;
        }

        return `
            <span class="chat-msg-reply" data-jump="${message.replyToMessageId}">
                <b>${esc(message.replyToSenderName)}</b>
                <span>${esc(message.replyToContent || "")}</span>
            </span>`;
    }

    function pinMarkup(message) {
        if (!message.isPinned) return "";
        return `<span class="chat-msg-pin-badge"><i class="fa-solid fa-thumbtack"></i> پین‌شده</span>`;
    }

    function reactionsMarkup(message) {
        if (!message.reactions || message.reactions.length === 0) return "";

        const items = message.reactions.map(r => {
            const reacted = r.userIds.includes(app.currentUserId);
            return `<button type="button" class="chat-reaction-chip ${reacted ? "reacted" : ""}" 
                        data-emoji="${esc(r.emoji)}" data-count="${r.count}"
                        title="${r.count} واکنش">
                        <span class="chat-reaction-emoji">${r.emoji}</span>
                        <span class="chat-reaction-count">${r.count}</span>
                    </button>`;
        }).join("");

        return `<div class="chat-msg-reactions">${items}<button type="button" class="chat-reaction-add" data-action="add-reaction" title="افزودن واکنش"><i class="fa-solid fa-face-smile"></i></button></div>`;
    }

    /** Highlight @mentions in message content */
    function highlightMentions(content) {
        if (!content) return esc(content);
        let escaped = esc(content);
        // Match @name patterns in the escaped text
        escaped = escaped.replace(/@([\p{L}\p{N}_]+)/gu, '<span class="chat-mention">@$1</span>');
        return escaped;
    }

    function messageMarkup(message, grouped) {
        const own = message.senderId === app.currentUserId;
        const hasText = message.content && message.content.trim().length > 0;

        const actions = `
            <span class="chat-msg-actions">
                <button type="button" class="chat-msg-action" data-action="add-reaction" title="واکنش">
                    <i class="fa-solid fa-face-smile"></i>
                </button>
                <button type="button" class="chat-msg-action" data-action="reply" title="پاسخ">
                    <i class="fa-solid fa-reply"></i>
                </button>
                ${!own && app.room.canManage || own
                    ? `<button type="button" class="chat-msg-action" data-action="pin" title="${message.isPinned ? "حذف pin" : "pin کردن"}"><i class="fa-solid fa-thumbtack"></i></button>`
                    : ""}
                ${own && message.typeName === "Text"
                    ? `<button type="button" class="chat-msg-action" data-action="edit" title="ویرایش"><i class="fa-solid fa-pen"></i></button>`
                    : ""}
                ${own || app.room.canManage
                    ? `<button type="button" class="chat-msg-action" data-action="delete" title="حذف"><i class="fa-solid fa-trash"></i></button>`
                    : ""}
            </span>`;

        return `
            <div class="chat-msg ${own ? "own" : "other"} ${grouped ? "grouped" : ""} ${message.isPinned ? "pinned" : ""}" data-id="${message.id}" data-sender="${message.senderId}">
                ${!own && !grouped ? avatarMarkup(message.senderName, message.senderAvatar) : `<span class="chat-msg-avatar-spacer"></span>`}
                <div class="chat-msg-bubble">
                    ${!own && !grouped ? `<span class="chat-msg-sender">${esc(message.senderName)}</span>` : ""}
                    ${pinMarkup(message)}
                    ${replyMarkup(message)}
                    ${message.typeName !== "Text" ? attachmentMarkup(message) : ""}
                    ${hasText ? `<span class="chat-msg-text">${highlightMentions(message.content)}</span>` : ""}
                    ${reactionsMarkup(message)}
                    <span class="chat-msg-meta">
                        ${message.isEdited ? `<i class="chat-msg-edited">ویرایش‌شده</i>` : ""}
                        <time>${esc(formatTime(message.createdDate))}</time>
                    </span>
                    ${actions}
                </div>
            </div>`;
    }

    function daySeparator(iso) {
        return `<div class="chat-day-separator"><span>${esc(formatDay(iso))}</span></div>`;
    }

    function isGrouped(message, previous) {
        if (!previous) return false;
        if (previous.senderId !== message.senderId) return false;
        if (message.replyToMessageId) return false;

        const gap = new Date(message.createdDate) - new Date(previous.createdDate);
        return gap < 5 * 60 * 1000;
    }

    function renderMessages() {
        if (!dom.body) return;

        if (app.messages.length === 0) {
            dom.body.innerHTML = `
                <div class="chat-empty-room">
                    <i class="fa-solid fa-comment-dots"></i>
                    <p>هنوز پیامی در این گروه ارسال نشده است.<br />اولین پیام را شما بفرستید!</p>
                </div>`;
            return;
        }

        let html = "";
        let lastDay = null;

        app.messages.forEach((message, index) => {
            const key = dayKey(new Date(message.createdDate));

            if (key !== lastDay) {
                html += daySeparator(message.createdDate);
                lastDay = key;
                html += messageMarkup(message, false);
            } else {
                html += messageMarkup(message, isGrouped(message, app.messages[index - 1]));
            }
        });

        dom.body.innerHTML = html;
    }

    function toggleLoadMore() {
        if (!dom.loadMore) return;
        dom.loadMore.classList.toggle("d-none", !app.hasMore);
    }

    // ===== Rendering: pinned messages bar =====

    function renderPinnedBar() {
        if (!dom.pinnedBar || !dom.pinnedList) return;

        const pinned = app.messages.filter(m => m.isPinned);

        if (pinned.length === 0) {
            dom.pinnedBar.classList.add("d-none");
            return;
        }

        dom.pinnedBar.classList.remove("d-none");
        dom.pinnedList.innerHTML = pinned.map(m => `
            <div class="chat-pinned-item" data-id="${m.id}" data-jump="${m.id}">
                <i class="fa-solid fa-thumbtack"></i>
                <span class="chat-pinned-sender">${esc(m.senderName)}</span>
                <span class="chat-pinned-text">${esc(m.content || m.attachmentName || "")}</span>
            </div>
        `).join("");
    }

    // ===== Rendering: members & presence =====

    function renderMembers() {
        if (!dom.membersList || !app.room) return;

        const members = app.room.members.slice().sort((a, b) => {
            const onlineDiff = (b.isOnline ? 1 : 0) - (a.isOnline ? 1 : 0);
            if (onlineDiff !== 0) return onlineDiff;
            return a.fullName.localeCompare(b.fullName, "fa");
        });

        dom.membersList.innerHTML = members.map(member => `
            <div class="chat-member ${member.isOnline ? "online" : ""}" data-user-id="${member.userId}">
                <span class="chat-member-avatar">
                    ${member.avatar
                        ? `<img src="${esc(member.avatar)}" alt="${esc(member.fullName)}" />`
                        : esc(initials(member.fullName))}
                    <span class="chat-presence-dot"></span>
                </span>
                <span class="chat-member-body">
                    <span class="chat-member-name">
                        ${esc(member.fullName)}
                        ${member.userId === app.currentUserId ? `<i class="chat-member-you">(شما)</i>` : ""}
                    </span>
                    <span class="chat-member-status">
                        ${member.isOnline ? "آنلاین" : esc(formatLastSeen(member.lastSeen))}
                    </span>
                </span>
                <span class="chat-member-role role-${esc((member.roleKey || "").toLowerCase())}">
                    ${esc(member.roleName || "")}
                </span>
            </div>`).join("");

        updateOnlineCount();
    }

    function updateOnlineCount() {
        if (!app.room) return;

        if (dom.memberCount) dom.memberCount.textContent = app.room.members.length;
        if (dom.onlineCount) dom.onlineCount.textContent = app.room.members.filter(x => x.isOnline).length;
    }

    function applyPresence() {
        if (!app.room) return;

        app.room.members.forEach(member => {
            member.isOnline = app.online.has(member.userId);
        });

        renderMembers();
    }

    function setPresence(userId, isOnline, lastSeen) {
        if (isOnline) app.online.add(userId);
        else app.online.delete(userId);

        if (!app.room) return;

        const member = app.room.members.find(x => x.userId === userId);
        if (!member) return;

        member.isOnline = isOnline;
        if (!isOnline) member.lastSeen = lastSeen || new Date().toISOString();

        renderMembers();
    }

    // ===== Rendering: chat list =====

    function listItem(projectId) {
        return dom.list
            ? dom.list.querySelector(`.chat-list-item[data-project-id="${projectId}"]`)
            : null;
    }

    function bumpToTop(item) {
        if (dom.list && item && dom.list.firstElementChild !== item) {
            dom.list.prepend(item);
        }
    }

    function updateListPreview(message) {
        const item = listItem(message.projectId);
        if (!item) return;

        const preview = item.querySelector(".chat-list-preview");
        const time = item.querySelector(".chat-list-time");

        if (preview) {
            const sender = message.senderId === app.currentUserId ? "شما" : message.senderName;
            let text = message.content;

            if (message.typeName === "Image") text = "🖼 تصویر";
            else if (message.typeName === "File") text = "📎 " + message.attachmentName;

            preview.innerHTML = `<b>${esc(sender)}:</b> ${esc(text)}`;
        }

        if (time) {
            time.dataset.time = message.createdDate;
            time.textContent = formatListTime(message.createdDate);
        }

        bumpToTop(item);
    }

    function setUnread(projectId, count) {
        const item = listItem(projectId);
        if (!item) return;

        const badge = item.querySelector(".chat-unread-badge");
        if (!badge) return;

        badge.dataset.count = count;
        badge.textContent = count > 99 ? "99+" : count;
        badge.classList.toggle("d-none", count <= 0);
    }

    function incrementUnread(projectId) {
        const item = listItem(projectId);
        if (!item) return;

        const badge = item.querySelector(".chat-unread-badge");
        if (!badge) return;

        const current = parseInt(badge.dataset.count || badge.textContent, 10) || 0;
        setUnread(projectId, current + 1);
    }

    function hydrateListTimes() {
        document.querySelectorAll(".chat-list-time[data-time]").forEach(el => {
            if (el.dataset.time) el.textContent = formatListTime(el.dataset.time);
        });
    }

    // ===== Emoji Picker =====

    function showEmojiPicker(anchorEl, onSelect) {
        closeEmojiPicker();

        const picker = document.createElement("div");
        picker.className = "chat-emoji-picker";
        picker.innerHTML = EMOJI_LIST.map(e =>
            `<button type="button" class="chat-emoji-option">${e}</button>`
        ).join("");

        document.body.appendChild(picker);

        const rect = anchorEl.getBoundingClientRect();
        picker.style.top = (rect.top - picker.offsetHeight - 8) + "px";
        picker.style.left = Math.min(rect.left, window.innerWidth - picker.offsetWidth - 16) + "px";

        picker.addEventListener("click", e => {
            const btn = e.target.closest(".chat-emoji-option");
            if (btn) {
                onSelect(btn.textContent.trim());
                closeEmojiPicker();
            }
        });

        dom.emojiPicker = picker;

        setTimeout(() => {
            document.addEventListener("click", closeEmojiPickerOutside, { once: true });
        }, 0);
    }

    function closeEmojiPicker() {
        if (dom.emojiPicker) {
            dom.emojiPicker.remove();
            dom.emojiPicker = null;
        }
    }

    function closeEmojiPickerOutside(e) {
        if (dom.emojiPicker && !dom.emojiPicker.contains(e.target) && !e.target.closest("[data-action='add-reaction']")) {
            closeEmojiPicker();
        }
    }

    // ===== Mention Autocomplete =====

    function showMentionDropdown(query) {
        closeMentionDropdown();

        if (!app.room || !app.room.members) return;

        const q = query.toLowerCase();
        const matches = app.room.members.filter(m =>
            m.fullName.toLowerCase().includes(q) && m.userId !== app.currentUserId
        ).slice(0, 6);

        if (matches.length === 0) return;

        app.mentionMembers = matches;

        const dropdown = document.createElement("div");
        dropdown.className = "chat-mention-dropdown";

        dropdown.innerHTML = matches.map((m, i) => `
            <div class="chat-mention-option ${i === 0 ? "selected" : ""}" data-user-id="${m.userId}" data-name="${esc(m.fullName)}">
                <span class="chat-mention-avatar">${m.avatar ? `<img src="${esc(m.avatar)}" alt="" />` : esc(initials(m.fullName))}</span>
                <span class="chat-mention-name">${esc(m.fullName)}</span>
            </div>
        `).join("");

        // Position near the textarea
        const inputRect = dom.input.getBoundingClientRect();
        dropdown.style.position = "fixed";
        dropdown.style.bottom = (window.innerHeight - inputRect.top + 8) + "px";
        dropdown.style.left = inputRect.left + "px";
        dropdown.style.right = (window.innerWidth - inputRect.right) + "px";

        document.body.appendChild(dropdown);
        dom.mentionDropdown = dropdown;
    }

    function closeMentionDropdown() {
        if (dom.mentionDropdown) {
            dom.mentionDropdown.remove();
            dom.mentionDropdown = null;
        }
        app.mentionQuery = null;
        app.mentionStart = -1;
        app.mentionMembers = [];
    }

    function insertMention(name) {
        const val = dom.input.value;
        const before = val.substring(0, app.mentionStart);
        const after = val.substring(dom.input.selectionStart);
        dom.input.value = before + "@" + name + " " + after;
        dom.input.focus();
        const pos = before.length + name.length + 2;
        dom.input.setSelectionRange(pos, pos);
        autoResize();
        closeMentionDropdown();
    }

    // ===== SignalR =====

    let connection = null;

    function showBanner(visible) {
        if (dom.banner) dom.banner.classList.toggle("d-none", !visible);
    }

    function appendIncoming(message) {
        const wasNearBottom = isNearBottom();

        app.messages.push(message);
        renderMessages();
        renderPinnedBar();

        if (wasNearBottom || message.senderId === app.currentUserId) {
            scrollToBottom();
        }
    }

    function onReceiveMessage(message) {
        updateListPreview(message);

        const isActive = app.room && message.projectId === app.room.projectId;

        if (!isActive) {
            if (message.senderId !== app.currentUserId) incrementUnread(message.projectId);
            return;
        }

        appendIncoming(message);
        clearTyping(message.senderId);

        if (message.senderId !== app.currentUserId && document.hasFocus()) {
            connection.invoke("MarkAsRead", message.projectId).catch(() => { });
        } else if (message.senderId !== app.currentUserId) {
            incrementUnread(message.projectId);
        }
    }

    function onMessageEdited(message) {
        if (!app.room || message.projectId !== app.room.projectId) return;

        const index = app.messages.findIndex(x => x.id === message.id);
        if (index === -1) return;

        app.messages[index] = message;
        renderMessages();
        renderPinnedBar();
    }

    function onMessageDeleted(payload) {
        if (!app.room || payload.projectId !== app.room.projectId) return;

        app.messages = app.messages.filter(x => x.id !== payload.messageId);

        app.messages.forEach(x => {
            if (x.replyToMessageId === payload.messageId) {
                x.replyToSenderName = null;
                x.replyToContent = null;
            }
        });

        renderMessages();
        renderPinnedBar();
    }

    function onReactionToggled(message) {
        if (!app.room || message.projectId !== app.room.projectId) return;

        const index = app.messages.findIndex(x => x.id === message.id);
        if (index === -1) return;

        app.messages[index] = message;
        renderMessages();
    }

    function onPinToggled(message) {
        if (!app.room || message.projectId !== app.room.projectId) return;

        const index = app.messages.findIndex(x => x.id === message.id);
        if (index === -1) return;

        app.messages[index] = message;
        renderMessages();
        renderPinnedBar();
    }

    function onMentionNotification(payload) {
        if (typeof Swal !== "undefined") {
            Swal.fire({
                icon: "info",
                title: "شما mention شدید",
                html: `<b>${esc(payload.senderName)}</b> شما را در پیامی ذکر کرد:<br/><i>"${esc(payload.content)}"</i>`,
                confirmButtonText: "مشاهده",
                timer: 6000,
                timerProgressBar: true
            }).then(result => {
                if (result.isConfirmed || result.dismiss === Swal.DismissReason.timer) {
                    if (app.room && app.room.projectId === payload.projectId) {
                        const el = dom.body.querySelector(`.chat-msg[data-id="${payload.messageId}"]`);
                        if (el) {
                            el.scrollIntoView({ behavior: "smooth", block: "center" });
                            el.classList.add("highlight");
                            setTimeout(() => el.classList.remove("highlight"), 1500);
                        }
                    }
                }
            });
        }
    }

    function clearTyping(userId) {
        const entry = app.typingTimers.get(userId);
        if (!entry) return;

        clearTimeout(entry.timer);
        app.typingTimers.delete(userId);
        renderTyping();
    }

    function renderTyping() {
        if (!dom.typing) return;

        const names = Array.from(app.typingTimers.values()).map(x => x.name);

        if (names.length === 0) {
            dom.typing.classList.add("d-none");
            if (dom.roomStatus) dom.roomStatus.classList.remove("d-none");
            return;
        }

        dom.typing.textContent = names.length === 1
            ? `${names[0]} در حال نوشتن...`
            : `${names.length} نفر در حال نوشتن...`;

        dom.typing.classList.remove("d-none");
        if (dom.roomStatus) dom.roomStatus.classList.add("d-none");
    }

    function onUserTyping(payload) {
        if (!app.room || payload.projectId !== app.room.projectId) return;
        if (payload.userId === app.currentUserId) return;

        const existing = app.typingTimers.get(payload.userId);
        if (existing) clearTimeout(existing.timer);

        if (!payload.isTyping) {
            app.typingTimers.delete(payload.userId);
            renderTyping();
            return;
        }

        const member = app.room.members.find(x => x.userId === payload.userId);
        const name = member ? member.fullName : payload.userName;

        const entry = { name };
        entry.timer = setTimeout(() => {
            app.typingTimers.delete(payload.userId);
            renderTyping();
        }, 4000);
        app.typingTimers.set(payload.userId, entry);

        renderTyping();
    }

    function startConnection() {
        if (typeof signalR === "undefined") return;

        connection = new signalR.HubConnectionBuilder()
            .withUrl("/hubs/chat")
            .withAutomaticReconnect([0, 2000, 5000, 10000, 20000])
            .build();

        connection.on("ReceiveMessage", onReceiveMessage);
        connection.on("MessageEdited", onMessageEdited);
        connection.on("MessageDeleted", onMessageDeleted);
        connection.on("UserTyping", onUserTyping);
        connection.on("ReactionToggled", onReactionToggled);
        connection.on("PinToggled", onPinToggled);
        connection.on("MentionNotification", onMentionNotification);

        connection.on("OnlineUsers", userIds => {
            app.online = new Set(userIds);
            applyPresence();
        });

        connection.on("UserOnline", payload => setPresence(payload.userId, true));
        connection.on("UserOffline", payload => setPresence(payload.userId, false, payload.lastSeen));

        connection.onreconnecting(() => showBanner(true));

        connection.onreconnected(() => {
            showBanner(false);
            if (app.room) reloadRoom(app.room.projectId, false);
        });

        connection.onclose(() => showBanner(true));

        connection.start()
            .then(() => {
                showBanner(false);
                if (app.room) connection.invoke("MarkAsRead", app.room.projectId).catch(() => { });
            })
            .catch(err => {
                console.error("Chat hub connection error:", err);
                showBanner(true);
            });
    }

    // ===== Room switching =====

    function applyRoom(room, pushUrl) {
        app.room = room;
        app.messages = room.messages || [];
        app.hasMore = room.hasMore;
        app.replyTo = null;
        app.editing = null;
        app.typingTimers.forEach(entry => clearTimeout(entry.timer));
        app.typingTimers.clear();

        if (dom.roomName) dom.roomName.textContent = room.projectName;

        if (dom.roomAvatar) {
            dom.roomAvatar.style.background = room.color || "#6366F1";
            dom.roomAvatar.innerHTML = `<i class="${esc(room.icon || "fa-solid fa-diagram-project")}"></i>`;
        }

        if (dom.list) {
            dom.list.querySelectorAll(".chat-list-item").forEach(item => {
                item.classList.toggle("active", parseInt(item.dataset.projectId, 10) === room.projectId);
            });
        }

        setUnread(room.projectId, 0);
        applyPresence();
        renderMessages();
        renderPinnedBar();
        renderTyping();
        toggleLoadMore();
        cancelReply();
        scrollToBottom();

        if (pushUrl) {
            history.pushState({ projectId: room.projectId }, "", `/Chat?projectId=${room.projectId}`);
        }

        document.title = `${room.projectName} - گفتگو - SmartTask`;
    }

    function reloadRoom(projectId, pushUrl) {
        if (app.loading) return;
        app.loading = true;

        fetch(`/Chat/Room?projectId=${projectId}`, { headers: { "X-Requested-With": "XMLHttpRequest" } })
            .then(response => {
                if (!response.ok) throw new Error("forbidden");
                return response.json();
            })
            .then(room => applyRoom(room, pushUrl))
            .catch(() => {
                if (typeof showError === "function") showError("امکان باز کردن این گفتگو وجود ندارد.");
            })
            .finally(() => { app.loading = false; });
    }

    function loadOlder() {
        if (!app.room || app.loading || app.messages.length === 0) return;

        app.loading = true;
        const oldestId = app.messages[0].id;
        const previousHeight = dom.messages.scrollHeight;

        fetch(`/Chat/History?projectId=${app.room.projectId}&beforeId=${oldestId}`,
            { headers: { "X-Requested-With": "XMLHttpRequest" } })
            .then(response => response.json())
            .then(data => {
                if (!data.messages || data.messages.length === 0) {
                    app.hasMore = false;
                    toggleLoadMore();
                    return;
                }

                app.messages = data.messages.concat(app.messages);
                app.hasMore = data.hasMore;

                renderMessages();
                toggleLoadMore();

                dom.messages.scrollTop = dom.messages.scrollHeight - previousHeight;
            })
            .catch(() => { })
            .finally(() => { app.loading = false; });
    }

    // ===== Composer =====

    function autoResize() {
        if (!dom.input) return;
        dom.input.style.height = "auto";
        dom.input.style.height = Math.min(dom.input.scrollHeight, 140) + "px";
    }

    function cancelReply() {
        app.replyTo = null;
        if (dom.replyPreview) dom.replyPreview.classList.add("d-none");
    }

    function startReply(message) {
        app.editing = null;
        app.replyTo = message.id;

        if (dom.replyPreview) {
            dom.replyPreview.classList.remove("d-none");
            dom.replyPreview.classList.remove("editing");
        }
        if (dom.replyName) dom.replyName.textContent = message.senderName;
        if (dom.replyText) {
            dom.replyText.textContent = message.typeName === "Text"
                ? message.content
                : (message.attachmentName || "پیوست");
        }

        dom.input.focus();
    }

    function startEdit(message) {
        app.replyTo = null;
        app.editing = message.id;

        if (dom.replyPreview) {
            dom.replyPreview.classList.remove("d-none");
            dom.replyPreview.classList.add("editing");
        }
        if (dom.replyName) dom.replyName.textContent = "ویرایش پیام";
        if (dom.replyText) dom.replyText.textContent = message.content;

        dom.input.value = message.content;
        autoResize();
        dom.input.focus();
    }

    function cancelEdit() {
        app.editing = null;
        dom.input.value = "";
        autoResize();
        if (dom.replyPreview) {
            dom.replyPreview.classList.add("d-none");
            dom.replyPreview.classList.remove("editing");
        }
    }

    function sendTyping(isTyping) {
        if (!connection || !app.room) return;
        if (connection.state !== signalR.HubConnectionState.Connected) return;

        connection.invoke("Typing", app.room.projectId, isTyping).catch(() => { });
    }

    function handleTypingInput() {
        if (!app.typingSent) {
            app.typingSent = true;
            sendTyping(true);
        }

        clearTimeout(app.typingTimeout);
        app.typingTimeout = setTimeout(() => {
            app.typingSent = false;
            sendTyping(false);
        }, 2500);
    }

    function stopTyping() {
        clearTimeout(app.typingTimeout);
        if (app.typingSent) {
            app.typingSent = false;
            sendTyping(false);
        }
    }

    function send() {
        if (!app.room || !connection) return;

        const text = dom.input.value.trim();
        if (!text) return;

        if (connection.state !== signalR.HubConnectionState.Connected) {
            if (typeof showError === "function") showError("ارتباط برقرار نیست. لطفاً کمی صبر کنید.");
            return;
        }

        stopTyping();
        closeMentionDropdown();

        if (app.editing) {
            const messageId = app.editing;
            connection.invoke("EditMessage", messageId, text)
                .then(() => cancelEdit())
                .catch(err => {
                    if (typeof showError === "function") showError(err.message || "ویرایش پیام ناموفق بود.");
                });
            return;
        }

        const replyTo = app.replyTo;

        dom.input.value = "";
        autoResize();
        cancelReply();

        connection.invoke("SendMessage", app.room.projectId, text, replyTo)
            .catch(err => {
                dom.input.value = text;
                autoResize();
                if (typeof showError === "function") showError(err.message || "ارسال پیام ناموفق بود.");
            });
    }

    // ===== Attachments =====

    function uploadFile(file) {
        if (!app.room || !file) return;

        if (file.size > 10 * 1024 * 1024) {
            if (typeof showError === "function") showError("حجم فایل نباید بیشتر از ۱۰ مگابایت باشد.");
            return;
        }

        const formData = new FormData();
        formData.append("projectId", app.room.projectId);
        formData.append("file", file);
        formData.append("caption", dom.input.value.trim());
        if (app.replyTo) formData.append("replyToMessageId", app.replyTo);
        formData.append("__RequestVerificationToken", token());

        const request = new XMLHttpRequest();
        request.open("POST", "/Chat/Upload");

        dom.uploadProgress.classList.remove("d-none");
        dom.uploadName.textContent = file.name;
        dom.progressFill.style.width = "0%";

        request.upload.addEventListener("progress", event => {
            if (event.lengthComputable) {
                dom.progressFill.style.width = Math.round((event.loaded / event.total) * 100) + "%";
            }
        });

        request.addEventListener("load", () => {
            dom.uploadProgress.classList.add("d-none");

            if (request.status >= 200 && request.status < 300) {
                dom.input.value = "";
                autoResize();
                cancelReply();
                return;
            }

            let message = "بارگذاری فایل ناموفق بود.";
            try {
                message = JSON.parse(request.responseText).message || message;
            } catch { }

            if (typeof showError === "function") showError(message);
        });

        request.addEventListener("error", () => {
            dom.uploadProgress.classList.add("d-none");
            if (typeof showError === "function") showError("بارگذاری فایل ناموفق بود.");
        });

        request.send(formData);
    }

    // ===== Message actions =====

    function findMessage(id) {
        return app.messages.find(x => x.id === id);
    }

    function deleteMessage(messageId) {
        const run = () => connection.invoke("DeleteMessage", messageId).catch(err => {
            if (typeof showError === "function") showError(err.message || "حذف پیام ناموفق بود.");
        });

        if (typeof Swal === "undefined") {
            run();
            return;
        }

        Swal.fire({
            title: "حذف پیام",
            text: "آیا از حذف این پیام مطمئن هستید؟",
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "بله، حذف کن",
            cancelButtonText: "انصراف",
            confirmButtonColor: "#EF4444",
            cancelButtonColor: "#64748B"
        }).then(result => {
            if (result.isConfirmed) run();
        });
    }

    function jumpTo(messageId) {
        const el = dom.body.querySelector(`.chat-msg[data-id="${messageId}"]`);
        if (!el) return;

        el.scrollIntoView({ behavior: "smooth", block: "center" });
        el.classList.add("highlight");
        setTimeout(() => el.classList.remove("highlight"), 1500);
    }

    // ===== Events =====

    function bindEvents() {

        // Chat list selection
        if (dom.list) {
            dom.list.addEventListener("click", event => {
                const item = event.target.closest(".chat-list-item");
                if (!item) return;

                const projectId = parseInt(item.dataset.projectId, 10);
                if (app.room && app.room.projectId === projectId) {
                    dom.appRoot.classList.remove("show-list");
                    return;
                }

                dom.appRoot.classList.remove("show-list");
                reloadRoom(projectId, true);
            });
        }

        // Filter chat list
        if (dom.listSearch) {
            dom.listSearch.addEventListener("input", () => {
                const term = dom.listSearch.value.trim().toLowerCase();

                dom.list.querySelectorAll(".chat-list-item").forEach(item => {
                    const name = (item.dataset.projectName || "").toLowerCase();
                    item.classList.toggle("d-none", term !== "" && !name.includes(term));
                });
            });
        }

        // Send message
        if (dom.sendBtn) dom.sendBtn.addEventListener("click", send);

        // Test push
        if (dom.testPushBtn) {
            dom.testPushBtn.addEventListener("click", async function () {
                if (!app.room || !connection || connection.state !== "Connected") {
                    if (typeof showWarning === "function") showWarning("اتصال برقرار نیست.");
                    return;
                }

                const btn = this;
                btn.disabled = true;

                try {
                    await connection.invoke("TestPush", app.room.projectId);

                    if (typeof Swal !== "undefined") {
                        Swal.fire({
                            icon: "success",
                            title: "اعلان آزمایشی ارسال شد",
                            text: "اگر مرورگر اعضا اشتراک داشته باشد، اعلان نمایش داده می‌شود.",
                            confirmButtonText: "باشه",
                            timer: 4000,
                            timerProgressBar: true
                        });
                    }
                } catch (err) {
                    console.error("Test push failed:", err);
                    if (typeof showError === "function") showError(err.message || "ارسال اعلان آزمایشی ناموفق بود.");
                } finally {
                    btn.disabled = false;
                }
            });
        }

        // Input with mention detection
        if (dom.input) {
            dom.input.addEventListener("keydown", event => {
                // Mention dropdown navigation
                if (dom.mentionDropdown) {
                    const options = dom.mentionDropdown.querySelectorAll(".chat-mention-option");
                    const selected = dom.mentionDropdown.querySelector(".chat-mention-option.selected");
                    const currentIdx = selected ? Array.from(options).indexOf(selected) : -1;

                    if (event.key === "ArrowDown" || event.key === "ArrowUp") {
                        event.preventDefault();
                        if (selected) selected.classList.remove("selected");
                        let nextIdx = event.key === "ArrowDown" ? currentIdx + 1 : currentIdx - 1;
                        if (nextIdx < 0) nextIdx = options.length - 1;
                        if (nextIdx >= options.length) nextIdx = 0;
                        options[nextIdx].classList.add("selected");
                        return;
                    }

                    if (event.key === "Enter" && selected) {
                        event.preventDefault();
                        insertMention(selected.dataset.name);
                        return;
                    }

                    if (event.key === "Escape") {
                        closeMentionDropdown();
                        return;
                    }
                }

                if (event.key === "Enter" && !event.shiftKey) {
                    event.preventDefault();
                    send();
                    return;
                }

                if (event.key === "Escape") {
                    if (app.editing) cancelEdit();
                    else cancelReply();
                }
            });

            dom.input.addEventListener("input", () => {
                autoResize();
                handleTypingInput();
                detectMention();
            });

            dom.input.addEventListener("blur", () => {
                stopTyping();
                // Delay mention close to allow click on option
                setTimeout(closeMentionDropdown, 200);
            });
        }

        // Mention dropdown clicks
        document.addEventListener("click", e => {
            const option = e.target.closest(".chat-mention-option");
            if (option) {
                insertMention(option.dataset.name);
            }
        });

        // Pinned bar clicks
        if (dom.pinnedBar) {
            dom.pinnedBar.addEventListener("click", e => {
                const item = e.target.closest(".chat-pinned-item");
                if (item) jumpTo(parseInt(item.dataset.id, 10));
            });
        }

        // File attachment
        if (dom.attachBtn) dom.attachBtn.addEventListener("click", () => dom.fileInput.click());

        if (dom.fileInput) {
            dom.fileInput.addEventListener("change", () => {
                if (dom.fileInput.files.length > 0) uploadFile(dom.fileInput.files[0]);
                dom.fileInput.value = "";
            });
        }

        // Drag and drop
        if (dom.messages) {
            ["dragenter", "dragover"].forEach(name => {
                dom.messages.addEventListener(name, event => {
                    event.preventDefault();
                    dom.messages.classList.add("dragging");
                });
            });

            ["dragleave", "drop"].forEach(name => {
                dom.messages.addEventListener(name, event => {
                    event.preventDefault();
                    dom.messages.classList.remove("dragging");
                });
            });

            dom.messages.addEventListener("drop", event => {
                if (event.dataTransfer.files.length > 0) uploadFile(event.dataTransfer.files[0]);
            });

            dom.messages.addEventListener("scroll", () => {
                if (dom.messages.scrollTop < 60 && app.hasMore) loadOlder();
            });
        }

        if (dom.loadMoreBtn) dom.loadMoreBtn.addEventListener("click", loadOlder);

        // Message actions (reactions, reply, edit, delete, pin)
        if (dom.body) {
            dom.body.addEventListener("click", event => {
                // Jump to replied message
                const jump = event.target.closest(".chat-msg-reply[data-jump]");
                if (jump) {
                    jumpTo(parseInt(jump.dataset.jump, 10));
                    return;
                }

                // Reaction chip click (toggle)
                const reactionChip = event.target.closest(".chat-reaction-chip");
                if (reactionChip) {
                    const msgEl = reactionChip.closest(".chat-msg");
                    const messageId = parseInt(msgEl.dataset.id, 10);
                    const emoji = reactionChip.dataset.emoji;
                    connection.invoke("ToggleReaction", messageId, emoji).catch(() => { });
                    return;
                }

                // Add reaction button
                const addReaction = event.target.closest("[data-action='add-reaction']");
                if (addReaction) {
                    const msgEl = addReaction.closest(".chat-msg");
                    const messageId = parseInt(msgEl.dataset.id, 10);
                    showEmojiPicker(addReaction, emoji => {
                        connection.invoke("ToggleReaction", messageId, emoji).catch(() => { });
                    });
                    return;
                }

                // Other actions
                const action = event.target.closest(".chat-msg-action");
                if (!action) return;

                const msgEl = action.closest(".chat-msg");
                const messageId = parseInt(msgEl.dataset.id, 10);
                const message = findMessage(messageId);
                if (!message) return;

                if (action.dataset.action === "reply") startReply(message);
                else if (action.dataset.action === "edit") startEdit(message);
                else if (action.dataset.action === "delete") deleteMessage(messageId);
                else if (action.dataset.action === "pin") {
                    connection.invoke("TogglePin", messageId).catch(err => {
                        if (typeof showError === "function") showError(err.message || "عملیات pin ناموفق بود.");
                    });
                }
            });
        }

        if (dom.replyCancel) {
            dom.replyCancel.addEventListener("click", () => {
                if (app.editing) cancelEdit();
                else cancelReply();
            });
        }

        // Members panel
        if (dom.membersToggle) {
            dom.membersToggle.addEventListener("click", () => dom.appRoot.classList.toggle("show-members"));
        }
        if (dom.membersClose) {
            dom.membersClose.addEventListener("click", () => dom.appRoot.classList.remove("show-members"));
        }

        // Search
        if (dom.searchToggle) {
            dom.searchToggle.addEventListener("click", () => {
                dom.searchBar.classList.toggle("d-none");
                if (!dom.searchBar.classList.contains("d-none")) dom.searchInput.focus();
            });
        }

        if (dom.searchClose) {
            dom.searchClose.addEventListener("click", () => {
                dom.searchBar.classList.add("d-none");
                dom.searchInput.value = "";
                app.searchSkip = 0;
                app.searchHasMore = false;
                filterMessages("");
            });
        }

        if (dom.searchInput) {
            let searchTimeout;
            dom.searchInput.addEventListener("input", () => {
                clearTimeout(searchTimeout);
                const term = dom.searchInput.value.trim();

                if (term.length < 2) {
                    filterMessages(term);
                    return;
                }

                searchTimeout = setTimeout(() => {
                    serverSearch(term);
                }, 300);
            });
        }

        // Mobile back
        if (dom.backBtn) {
            dom.backBtn.addEventListener("click", () => dom.appRoot.classList.add("show-list"));
        }

        // Mark as read on focus
        window.addEventListener("focus", () => {
            if (app.room && connection && connection.state === signalR.HubConnectionState.Connected) {
                connection.invoke("MarkAsRead", app.room.projectId).catch(() => { });
                setUnread(app.room.projectId, 0);
            }
        });

        window.addEventListener("popstate", event => {
            const projectId = event.state && event.state.projectId;
            if (projectId) reloadRoom(projectId, false);
        });
    }

    // ===== Mention detection =====

    function detectMention() {
        const val = dom.input.value;
        const cursorPos = dom.input.selectionStart;

        // Find @ before cursor
        const textBefore = val.substring(0, cursorPos);
        const lastAtIndex = textBefore.lastIndexOf("@");

        if (lastAtIndex === -1 || (lastAtIndex > 0 && textBefore[lastAtIndex - 1] !== " " && textBefore[lastAtIndex - 1] !== "\n")) {
            closeMentionDropdown();
            return;
        }

        const query = textBefore.substring(lastAtIndex + 1);

        // If there's a space after the mention, close it
        if (query.includes(" ")) {
            closeMentionDropdown();
            return;
        }

        app.mentionStart = lastAtIndex;
        app.mentionQuery = query;

        showMentionDropdown(query);
    }

    // ===== Server search =====

    function serverSearch(term) {
        if (!app.room || app.searchLoading) return;

        app.searchLoading = true;
        app.searchSkip = 0;

        fetch(`/Chat/Search?projectId=${app.room.projectId}&term=${encodeURIComponent(term)}&skip=0&take=50`,
            { headers: { "X-Requested-With": "XMLHttpRequest" } })
            .then(response => response.json())
            .then(data => {
                app.searchSkip = data.nextSkip || 0;
                app.searchHasMore = data.hasMore || false;

                // Replace messages with search results
                app.messages = data.messages || [];
                app.hasMore = false;
                renderMessages();
                toggleLoadMore();
            })
            .catch(() => { })
            .finally(() => { app.searchLoading = false; });
    }

    // ===== Client-side filter =====

    function filterMessages(term) {
        if (!dom.body) return;

        const lower = term.toLowerCase();

        dom.body.querySelectorAll(".chat-msg").forEach(el => {
            if (!term) {
                el.classList.remove("d-none", "search-hit");
                return;
            }

            const text = el.querySelector(".chat-msg-text");
            const match = text && text.textContent.toLowerCase().includes(lower);

            el.classList.toggle("d-none", !match);
            el.classList.toggle("search-hit", !!match);
        });

        dom.body.querySelectorAll(".chat-day-separator").forEach(el => {
            el.classList.toggle("d-none", !!term);
        });
    }

    // ===== Init =====

    hydrateListTimes();
    bindEvents();

    if (app.room) {
        app.messages = app.room.messages || [];
        app.hasMore = app.room.hasMore;

        renderMessages();
        renderMembers();
        renderPinnedBar();
        toggleLoadMore();
        scrollToBottom();
    }

    startConnection();
})();
