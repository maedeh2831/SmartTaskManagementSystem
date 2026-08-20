import React, { useState } from 'react';
import './SmartTaskSimple.css';

// Ultra-Simple Task Management UI Component
export default function SmartTaskSimple() {
  const [tasks, setTasks] = useState([
    { id: 1, title: 'مطالعه فصل۱', priority: 3, dueDate: '1403-10-14', status: 'todo' },
    { id: 2, title: 'انجام تمرین ریاضی', priority: 2, dueDate: '1403-10-15', status: 'in-progress' },
    { id: 3, title: 'آماده‌سازی پروژه', priority: 1, dueDate: '1403-10-16', status: 'todo' },
  ]);

  const [activeTab, setActiveTab] = useState('tasks');
  const [showAddModal, setShowAddModal] = useState(false);
  const [newTask, setNewTask] = useState({ title: '', priority: 2, dueDate: '' });

  // Add task
  const handleAddTask = (e) => {
    e.preventDefault();
    if (newTask.title.trim()) {
      setTasks([...tasks, {
        id: Date.now(),
        title: newTask.title,
        priority: newTask.priority,
        dueDate: newTask.dueDate,
        status: 'todo',
      }]);
      setNewTask({ title: '', priority: 2, dueDate: '' });
      setShowAddModal(false);
    }
  };

  // Delete task
  const handleDeleteTask = (id) => {
    if (confirm('آیا از حذف اطمینان دارید؟')) {
      setTasks(tasks.filter(t => t.id !== id));
    }
  };

  // Mark as done
  const handleComplete = (id) => {
    setTasks(tasks.map(t =>
      t.id === id ? { ...t, status: t.status === 'done' ? 'todo' : 'done' } : t
    ));
  };

  // Change status
  const handleChangeStatus = (id, status) => {
    setTasks(tasks.map(t => t.id === id ? { ...t, status } : t));
  };

  // Render priority stars
  const renderStars = (priority) => {
    return '★'.repeat(priority) + '☆'.repeat(5 - priority);
  };

  // Render status badge
  const getStatusLabel = (status) => {
    const labels = {
      'todo': 'انتظار',
      'in-progress': 'در حال انجام',
      'done': 'تمام شده'
    };
    return labels[status] || status;
  };

  const getStatusColor = (status) => {
    const colors = {
      'todo': '#FFC107',
      'in-progress': '#17A2B8',
      'done': '#28A745'
    };
    return colors[status] || '#F8F9FA';
  };

  return (
    <div className="smart-task-simple">
      {/* Header */}
      <header className="header">
        <h1>✅ SmartTask</h1>
        <p>مدیریت ساده و سریع وظایف</p>
      </header>

      {/* Main Content */}
      <main className="main-content">
        {activeTab === 'tasks' && (
          <div className="tasks-container">
            {/* Add Task Button */}
            <button
              className="btn btn-primary btn-large"
              onClick={() => setShowAddModal(true)}
            >
              ➕ افزودن وظیفهٔ جدید
            </button>

            {/* Task List */}
            <div className="task-list">
              {tasks.length === 0 ? (
                <div className="empty-state">
                  <div className="empty-icon">📭</div>
                  <h2>هنوز وظیفه‌ای نیست</h2>
                  <p>برای شروع، یک وظیفهٔ جدید ایجاد کنید</p>
                </div>
              ) : (
                tasks.map(task => (
                  <div key={task.id} className="task-card">
                    {/* Task Header */}
                    <div className="task-header">
                      <input
                        type="checkbox"
                        className="task-checkbox"
                        checked={task.status === 'done'}
                        onChange={() => handleComplete(task.id)}
                      />
                      <h3 className={task.status === 'done' ? 'task-title done' : 'task-title'}>
                        {task.title}
                      </h3>
                    </div>

                    {/* Task Meta */}
                    <div className="task-meta">
                      <div className="meta-item">
                        <span className="meta-label">اولویت:</span>
                        <span className="priority-stars">{renderStars(task.priority)}</span>
                      </div>
                      <div className="meta-item">
                        <span className="meta-label">تاریخ:</span>
                        <span className="meta-value">📅 {task.dueDate}</span>
                      </div>
                      <div className="meta-item">
                        <span className="meta-label">وضعیت:</span>
                        <span
                          className="status-badge"
                          style={{ backgroundColor: getStatusColor(task.status) }}
                        >
                          {getStatusLabel(task.status)}
                        </span>
                      </div>
                    </div>

                    {/* Status Selector */}
                    <div className="task-status-selector">
                      <button
                        className={`status-btn ${task.status === 'todo' ? 'active' : ''}`}
                        onClick={() => handleChangeStatus(task.id, 'todo')}
                      >
                        📝 انتظار
                      </button>
                      <button
                        className={`status-btn ${task.status === 'in-progress' ? 'active' : ''}`}
                        onClick={() => handleChangeStatus(task.id, 'in-progress')}
                      >
                        ⟳ در حال انجام
                      </button>
                      <button
                        className={`status-btn ${task.status === 'done' ? 'active' : ''}`}
                        onClick={() => handleChangeStatus(task.id, 'done')}
                      >
                        ✓ تمام
                      </button>
                    </div>

                    {/* Action Buttons */}
                    <div className="task-actions">
                      <button className="btn btn-secondary" title="ویرایش">
                        ✎ ویرایش
                      </button>
                      <button
                        className="btn btn-danger"
                        onClick={() => handleDeleteTask(task.id)}
                        title="حذف"
                      >
                        🗑 حذف
                      </button>
                    </div>
                  </div>
                ))
              )}
            </div>
          </div>
        )}

        {activeTab === 'stats' && (
          <div className="stats-container">
            <div className="stat-card">
              <div className="stat-number">{tasks.length}</div>
              <div className="stat-label">کل وظایف</div>
            </div>
            <div className="stat-card">
              <div className="stat-number">{tasks.filter(t => t.status === 'done').length}</div>
              <div className="stat-label">تمام شده</div>
            </div>
            <div className="stat-card">
              <div className="stat-number">{tasks.filter(t => t.status === 'in-progress').length}</div>
              <div className="stat-label">در حال انجام</div>
            </div>
            <div className="stat-card">
              <div className="stat-number">{tasks.filter(t => t.status === 'todo').length}</div>
              <div className="stat-label">انتظار</div>
            </div>
          </div>
        )}

        {activeTab === 'settings' && (
          <div className="settings-container">
            <h2>تنظیمات</h2>
            <div className="setting-item">
              <label>حالت تاریک</label>
              <input type="checkbox" className="toggle" />
            </div>
            <div className="setting-item">
              <label>اخطار برای تاریخ سررسید</label>
              <input type="checkbox" className="toggle" defaultChecked />
            </div>
            <div className="setting-item">
              <label>نمایش اولویت</label>
              <input type="checkbox" className="toggle" defaultChecked />
            </div>
            <button className="btn btn-danger">🔌 خروج</button>
          </div>
        )}
      </main>

      {/* Add Task Modal */}
      {showAddModal && (
        <div className="modal-overlay" onClick={() => setShowAddModal(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2>وظیفهٔ جدید</h2>
              <button className="btn-close" onClick={() => setShowAddModal(false)}>✕</button>
            </div>
            <form onSubmit={handleAddTask} className="modal-form">
              <div className="form-group">
                <label htmlFor="task-title">نام وظیفه</label>
                <input
                  id="task-title"
                  type="text"
                  placeholder="مثال: مطالعه فصل۱"
                  value={newTask.title}
                  onChange={(e) => setNewTask({ ...newTask, title: e.target.value })}
                  autoFocus
                />
              </div>

              <div className="form-group">
                <label htmlFor="task-priority">اولویت</label>
                <select
                  id="task-priority"
                  value={newTask.priority}
                  onChange={(e) => setNewTask({ ...newTask, priority: parseInt(e.target.value) })}
                >
                  <option value={1}>★ کم</option>
                  <option value={2}>★★ متوسط</option>
                  <option value={3}>★★★ زیاد</option>
                  <option value={4}>★★★★ خیلی زیاد</option>
                  <option value={5}>★★★★★ بحرانی</option>
                </select>
              </div>

              <div className="form-group">
                <label htmlFor="task-date">تاریخ سررسید</label>
                <input
                  id="task-date"
                  type="date"
                  value={newTask.dueDate}
                  onChange={(e) => setNewTask({ ...newTask, dueDate: e.target.value })}
                />
              </div>

              <div className="form-actions">
                <button type="submit" className="btn btn-primary btn-large">
                  ✓ افزودن
                </button>
                <button
                  type="button"
                  className="btn btn-secondary"
                  onClick={() => setShowAddModal(false)}
                >
                  لغو
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Bottom Navigation */}
      <nav className="bottom-nav">
        <button
          className={`nav-item ${activeTab === 'tasks' ? 'active' : ''}`}
          onClick={() => setActiveTab('tasks')}
          title="وظایف"
        >
          <span className="nav-icon">✅</span>
          <span className="nav-label">وظایف</span>
        </button>
        <button
          className={`nav-item ${activeTab === 'stats' ? 'active' : ''}`}
          onClick={() => setActiveTab('stats')}
          title="آمار"
        >
          <span className="nav-icon">📊</span>
          <span className="nav-label">آمار</span>
        </button>
        <button
          className={`nav-item ${activeTab === 'settings' ? 'active' : ''}`}
          onClick={() => setActiveTab('settings')}
          title="تنظیمات"
        >
          <span className="nav-icon">⚙️</span>
          <span className="nav-label">تنظیمات</span>
        </button>
      </nav>
    </div>
  );
}
