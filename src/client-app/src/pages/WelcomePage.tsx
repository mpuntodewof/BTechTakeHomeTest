import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Button,
  Grid,
  Container,
} from '@mui/material';
import {
  SendOutlined,
  HistoryOutlined,
  PeopleOutlined,
  ListAltOutlined,
} from '@mui/icons-material';
import { useAuth } from '../hooks/useAuth';

const formatCurrency = (amount: number): string => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
  }).format(amount);
};

export default function WelcomePage() {
  const navigate = useNavigate();
  const { user, isAdmin, refreshUser } = useAuth();

  useEffect(() => {
    refreshUser();
  }, [refreshUser]);

  return (
    <Container maxWidth="md" sx={{ py: 4 }}>
      <Typography variant="h4" fontWeight="bold" gutterBottom>
        Hello {user?.email}, welcome back
      </Typography>

      <Card sx={{ mb: 4 }}>
        <CardContent sx={{ textAlign: 'center', py: 4 }}>
          <Typography variant="body1" color="text.secondary" gutterBottom>
            Current Balance
          </Typography>
          <Typography variant="h3" fontWeight="bold" color="primary.main">
            {formatCurrency(user?.balance ?? 0)}
          </Typography>
        </CardContent>
      </Card>

      <Grid container spacing={3}>
        <Grid item xs={12} sm={6}>
          <Card
            sx={{
              cursor: 'pointer',
              transition: 'transform 0.2s, box-shadow 0.2s',
              '&:hover': { transform: 'translateY(-4px)', boxShadow: 4 },
            }}
            onClick={() => navigate('/transfer')}
          >
            <CardContent sx={{ textAlign: 'center', py: 4 }}>
              <SendOutlined sx={{ fontSize: 48, color: 'primary.main', mb: 1 }} />
              <Typography variant="h6">Transfer Funds</Typography>
              <Typography variant="body2" color="text.secondary">
                Send money to another user
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6}>
          <Card
            sx={{
              cursor: 'pointer',
              transition: 'transform 0.2s, box-shadow 0.2s',
              '&:hover': { transform: 'translateY(-4px)', boxShadow: 4 },
            }}
            onClick={() => navigate('/transactions')}
          >
            <CardContent sx={{ textAlign: 'center', py: 4 }}>
              <HistoryOutlined sx={{ fontSize: 48, color: 'secondary.main', mb: 1 }} />
              <Typography variant="h6">Transaction History</Typography>
              <Typography variant="body2" color="text.secondary">
                View your past transactions
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        {isAdmin && (
          <>
            <Grid item xs={12} sm={6}>
              <Card
                sx={{
                  cursor: 'pointer',
                  transition: 'transform 0.2s, box-shadow 0.2s',
                  '&:hover': { transform: 'translateY(-4px)', boxShadow: 4 },
                }}
                onClick={() => navigate('/users')}
              >
                <CardContent sx={{ textAlign: 'center', py: 4 }}>
                  <PeopleOutlined
                    sx={{ fontSize: 48, color: 'warning.main', mb: 1 }}
                  />
                  <Typography variant="h6">Manage Users</Typography>
                  <Typography variant="body2" color="text.secondary">
                    View and manage all users
                  </Typography>
                </CardContent>
              </Card>
            </Grid>

            <Grid item xs={12} sm={6}>
              <Card
                sx={{
                  cursor: 'pointer',
                  transition: 'transform 0.2s, box-shadow 0.2s',
                  '&:hover': { transform: 'translateY(-4px)', boxShadow: 4 },
                }}
                onClick={() => navigate('/transactions/all')}
              >
                <CardContent sx={{ textAlign: 'center', py: 4 }}>
                  <ListAltOutlined
                    sx={{ fontSize: 48, color: 'info.main', mb: 1 }}
                  />
                  <Typography variant="h6">All Transactions</Typography>
                  <Typography variant="body2" color="text.secondary">
                    View all system transactions
                  </Typography>
                </CardContent>
              </Card>
            </Grid>
          </>
        )}
      </Grid>
    </Container>
  );
}
