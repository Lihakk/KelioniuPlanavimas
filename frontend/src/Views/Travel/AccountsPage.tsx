import { useEffect, useState } from 'react';
import api from '../../api/api';
import type { UserAccount } from '../../types';

export const AccountsPage = () => {
    const [accounts, setAccounts] = useState<UserAccount[]>([]);
    const [currentUser, setCurrentUser] = useState<UserAccount | null>(null);
    const [mode, setMode] = useState<'login' | 'register'>('login');
    const [firstName, setFirstName] = useState('Linas');
    const [lastName, setLastName] = useState('Jancauskas');
    const [email, setEmail] = useState('linas@example.com');
    const [password, setPassword] = useState('password1');
    const [isAdmin, setIsAdmin] = useState(false);
    const [message, setMessage] = useState('');

    const loadAccounts = () => api.get<UserAccount[]>('/Account/administrateAccounts')
        .then(res => setAccounts(res.data))
        .catch(() => setAccounts([]));

    const submitAccount = async (e: React.FormEvent) => {
        e.preventDefault();
        setMessage('');

        try {
            const payload = { firstName, lastName, email, password, isAdmin };
            const response = mode === 'register'
                ? await api.post<UserAccount>('/Account/register', payload)
                : await api.post<UserAccount>('/Account/login', { email, password });

            setCurrentUser(response.data);
            setFirstName(response.data.firstName);
            setLastName(response.data.lastName);
            setEmail(response.data.email);
            setMessage(mode === 'register' ? 'Registration successful.' : 'Login successful.');
            await loadAccounts();
        } catch (error) {
            console.error(error);
            setMessage(mode === 'register' ? 'Registration failed.' : 'Login failed.');
        }
    };

    const saveProfile = async () => {
        if (!currentUser?.id) return;
        await api.put(`/Account/profile/${currentUser.id}`, { firstName, lastName, email });
        const profile = await api.get<UserAccount>(`/Account/profile/${currentUser.id}`);
        setCurrentUser(profile.data);
        setMessage('Profile updated.');
        await loadAccounts();
    };

    const logout = async () => {
        await api.post('/Account/logout');
        setCurrentUser(null);
        setMessage('User logged out.');
    };

    const changeStatus = async (account: UserAccount, action: 'block' | 'unblock') => {
        if (!account.id) return;
        await api.post(`/Account/${account.id}/${action}`);
        await loadAccounts();
    };

    useEffect(() => { loadAccounts(); }, []);

    return (
        <div className="workspace">
            <header className="page-head">
                <span className="eyebrow">Account diagrams K3-K7 and A1</span>
                <h1>Account, profile and administration</h1>
            </header>

            <div className="flow-grid">
                <section className="panel">
                    <div className="segmented">
                        <button className={mode === 'login' ? 'active' : ''} onClick={() => setMode('login')}>Login</button>
                        <button className={mode === 'register' ? 'active' : ''} onClick={() => setMode('register')}>Register</button>
                    </div>

                    <form onSubmit={submitAccount}>
                        {mode === 'register' && (
                            <div className="date-row">
                                <div>
                                    <label>First name</label>
                                    <input value={firstName} onChange={e => setFirstName(e.target.value)} required />
                                </div>
                                <div>
                                    <label>Last name</label>
                                    <input value={lastName} onChange={e => setLastName(e.target.value)} required />
                                </div>
                            </div>
                        )}

                        <label>Email</label>
                        <input type="email" value={email} onChange={e => setEmail(e.target.value)} required />

                        <label>Password</label>
                        <input type="password" value={password} onChange={e => setPassword(e.target.value)} required />

                        {mode === 'register' && (
                            <label className="check-row">
                                <input type="checkbox" checked={isAdmin} onChange={e => setIsAdmin(e.target.checked)} />
                                Administrator account
                            </label>
                        )}

                        <button className="btn btn-primary" type="submit">{mode === 'register' ? 'Create account' : 'Login'}</button>
                    </form>

                    {message && <p className="status-line">{message}</p>}
                </section>

                <section className="panel">
                    <div className="section-title">
                        <h2>Profile</h2>
                        {currentUser && <button className="btn btn-outline" onClick={logout}>Logout</button>}
                    </div>
                    {currentUser ? (
                        <>
                            <div className="date-row">
                                <div>
                                    <label>First name</label>
                                    <input value={firstName} onChange={e => setFirstName(e.target.value)} />
                                </div>
                                <div>
                                    <label>Last name</label>
                                    <input value={lastName} onChange={e => setLastName(e.target.value)} />
                                </div>
                            </div>
                            <label>Email</label>
                            <input value={email} onChange={e => setEmail(e.target.value)} />
                            <button className="btn btn-primary" onClick={saveProfile}>Save profile</button>
                        </>
                    ) : (
                        <div className="empty-state">Login or register to open profile view.</div>
                    )}
                </section>
            </div>

            <section className="panel">
                <div className="section-title">
                    <h2>User management</h2>
                    <button className="btn btn-outline" onClick={loadAccounts}>Refresh</button>
                </div>
                <div className="data-list dense">
                    {accounts.map(account => (
                        <article className="data-card" key={account.id}>
                            <strong>{account.firstName} {account.lastName}</strong>
                            <span>{account.email}</span>
                            <span>{account.isAdmin ? 'Administrator' : 'User'} | {account.accountStatus}</span>
                            <div className="inline-actions">
                                <button className="btn btn-outline" onClick={() => changeStatus(account, 'unblock')}>Unblock</button>
                                <button className="btn btn-danger" onClick={() => changeStatus(account, 'block')}>Block</button>
                            </div>
                        </article>
                    ))}
                </div>
            </section>
        </div>
    );
};
