import { BrowserRouter, Routes, Route, useNavigate } from 'react-router-dom';
import {
  AppBar,
  Toolbar,
  Typography,
  Button,
  Box,
  CssBaseline,
  ThemeProvider,
  createTheme,
} from '@mui/material';
import { AccountBalanceWallet, LogoutOutlined } from '@mui/icons-material';
import { AuthProvider } from './context/AuthContext';
import { useAuth } from './hooks/useAuth';
import ProtectedRoute from './components/ProtectedRoute';
import AdminRoute from './components/AdminRoute';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import WelcomePage from './pages/WelcomePage';
import TransferPage from './pages/TransferPage';
import TransactionHistoryPage from './pages/TransactionHistoryPage';
import AllTransactionsPage from './pages/AllTransactionsPage';
import UsersPage from './pages/UsersPage';
import AccessDeniedPage from './pages/AccessDeniedPage';

const theme = createTheme({
  palette: {
    primary: {
      main: '#1976d2',
    },
    secondary: {
      main: '#9c27b0',
    },
  },
  typography: {
    fontFamily: '"Roboto", "Helvetica", "Arial", sans-serif',
  },
});

function NavBar() {
  const { isAuthenticated, user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  if (!isAuthenticated) return null;

  return (
    <AppBar position="static" elevation={1}>
      <Toolbar>
        <AccountBalanceWallet sx={{ mr: 1 }} />
        <Typography
          variant="h6"
          component="div"
          sx={{ cursor: 'pointer' }}
          onClick={() => navigate('/')}
        >
          Fund Transfer
        </Typography>
        <Box sx={{ flexGrow: 1 }} />
        <Typography variant="body1" sx={{ mr: 2 }}>
          {user?.fullName}
        </Typography>
        <Button
          color="inherit"
          onClick={handleLogout}
          startIcon={<LogoutOutlined />}
        >
          Logout
        </Button>
      </Toolbar>
    </AppBar>
  );
}

function AppRoutes() {
  return (
    <>
      <NavBar />
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route path="/access-denied" element={<AccessDeniedPage />} />

        <Route element={<ProtectedRoute />}>
          <Route path="/" element={<WelcomePage />} />
          <Route path="/transfer" element={<TransferPage />} />
          <Route path="/transactions" element={<TransactionHistoryPage />} />
        </Route>

        <Route element={<AdminRoute />}>
          <Route path="/transactions/all" element={<AllTransactionsPage />} />
          <Route path="/users" element={<UsersPage />} />
        </Route>
      </Routes>
    </>
  );
}

export default function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <BrowserRouter>
        <AuthProvider>
          <AppRoutes />
        </AuthProvider>
      </BrowserRouter>
    </ThemeProvider>
  );
}
