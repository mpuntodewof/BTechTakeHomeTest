import { Link as RouterLink } from 'react-router-dom';
import { Container, Alert, AlertTitle, Button, Box } from '@mui/material';
import { HomeOutlined } from '@mui/icons-material';

export default function AccessDeniedPage() {
  return (
    <Container maxWidth="sm" sx={{ py: 8 }}>
      <Alert severity="error" variant="outlined">
        <AlertTitle>Access Denied</AlertTitle>
        You do not have permission to access this page. This area is restricted
        to administrators only.
      </Alert>
      <Box sx={{ mt: 3, textAlign: 'center' }}>
        <Button
          component={RouterLink}
          to="/"
          variant="contained"
          startIcon={<HomeOutlined />}
        >
          Back to Home
        </Button>
      </Box>
    </Container>
  );
}
