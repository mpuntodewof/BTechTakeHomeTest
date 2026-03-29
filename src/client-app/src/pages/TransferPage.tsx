import { useState, useEffect, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box,
  Card,
  CardContent,
  TextField,
  Button,
  Typography,
  Alert,
  Snackbar,
  CircularProgress,
  Container,
  InputAdornment,
} from '@mui/material';
import { SendOutlined } from '@mui/icons-material';
import { useAuth } from '../hooks/useAuth';
import api from '../services/api';
import { AxiosError } from 'axios';

const formatCurrency = (amount: number): string => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
  }).format(amount);
};

export default function TransferPage() {
  const navigate = useNavigate();
  const { user, refreshUser } = useAuth();

  const [recipientEmail, setRecipientEmail] = useState('');
  const [amount, setAmount] = useState('');
  const [notes, setNotes] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [successOpen, setSuccessOpen] = useState(false);

  useEffect(() => {
    refreshUser();
  }, [refreshUser]);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      await api.post('/api/transactions/transfer', {
        recipientEmail,
        amount: parseFloat(amount),
        notes: notes || undefined,
      });

      setSuccessOpen(true);
      await refreshUser();
      setRecipientEmail('');
      setAmount('');
      setNotes('');

      setTimeout(() => navigate('/'), 2000);
    } catch (err) {
      if (err instanceof AxiosError && err.response?.data) {
        const data = err.response.data;
        setError(
          typeof data === 'string'
            ? data
            : data.message || data.title || 'Transfer failed. Please try again.'
        );
      } else {
        setError('Transfer failed. Please try again.');
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <Container maxWidth="sm" sx={{ py: 4 }}>
      <Typography variant="h4" fontWeight="bold" gutterBottom>
        Transfer Funds
      </Typography>

      <Card sx={{ mb: 3 }}>
        <CardContent sx={{ textAlign: 'center' }}>
          <Typography variant="body2" color="text.secondary">
            Available Balance
          </Typography>
          <Typography variant="h5" fontWeight="bold" color="primary.main">
            {formatCurrency(user?.balance ?? 0)}
          </Typography>
        </CardContent>
      </Card>

      <Card>
        <CardContent sx={{ p: 3 }}>
          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}

          <Box component="form" onSubmit={handleSubmit}>
            <TextField
              label="Recipient Email"
              type="email"
              fullWidth
              required
              margin="normal"
              value={recipientEmail}
              onChange={(e) => setRecipientEmail(e.target.value)}
              placeholder="recipient@example.com"
              autoFocus
            />
            <TextField
              label="Amount"
              type="number"
              fullWidth
              required
              margin="normal"
              value={amount}
              onChange={(e) => setAmount(e.target.value)}
              slotProps={{
                input: {
                  startAdornment: (
                    <InputAdornment position="start">$</InputAdornment>
                  ),
                },
                htmlInput: { min: '0.01', step: '0.01' },
              }}
            />
            <TextField
              label="Notes (optional)"
              fullWidth
              margin="normal"
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              multiline
              rows={2}
              placeholder="Add a note for the recipient..."
            />

            <Box sx={{ display: 'flex', gap: 2, mt: 3 }}>
              <Button
                variant="outlined"
                fullWidth
                onClick={() => navigate('/')}
              >
                Cancel
              </Button>
              <Button
                type="submit"
                variant="contained"
                fullWidth
                disabled={loading}
                startIcon={
                  loading ? <CircularProgress size={20} /> : <SendOutlined />
                }
              >
                {loading ? 'Sending...' : 'Send Transfer'}
              </Button>
            </Box>
          </Box>
        </CardContent>
      </Card>

      <Snackbar
        open={successOpen}
        autoHideDuration={3000}
        onClose={() => setSuccessOpen(false)}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <Alert
          onClose={() => setSuccessOpen(false)}
          severity="success"
          variant="filled"
          sx={{ width: '100%' }}
        >
          Transfer completed successfully!
        </Alert>
      </Snackbar>
    </Container>
  );
}
