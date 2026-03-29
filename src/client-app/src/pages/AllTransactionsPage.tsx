import { useState, useEffect, useCallback } from 'react';
import {
  Container,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  TablePagination,
  CircularProgress,
  Box,
  Alert,
} from '@mui/material';
import api from '../services/api';
import type { TransactionListResponse, TransactionResponse } from '../types';

const formatCurrency = (amount: number): string => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
  }).format(amount);
};

const formatDate = (dateStr: string): string => {
  return new Date(dateStr).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
};

export default function AllTransactionsPage() {
  const [transactions, setTransactions] = useState<TransactionResponse[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(10);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const fetchTransactions = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const response = await api.get<TransactionListResponse>(
        '/api/transactions/all',
        { params: { page: page + 1, pageSize } }
      );
      setTransactions(response.data.items);
      setTotalCount(response.data.totalCount);
    } catch {
      setError('Failed to load transactions.');
    } finally {
      setLoading(false);
    }
  }, [page, pageSize]);

  useEffect(() => {
    fetchTransactions();
  }, [fetchTransactions]);

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Typography variant="h4" fontWeight="bold" gutterBottom>
        All Transactions (Admin)
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
          <CircularProgress />
        </Box>
      ) : transactions.length === 0 ? (
        <Paper sx={{ p: 4, textAlign: 'center' }}>
          <Typography color="text.secondary">
            No transactions found.
          </Typography>
        </Paper>
      ) : (
        <Paper>
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Date</TableCell>
                  <TableCell>Sender</TableCell>
                  <TableCell>Recipient</TableCell>
                  <TableCell align="right">Amount</TableCell>
                  <TableCell>Notes</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {transactions.map((tx) => (
                  <TableRow key={tx.id} hover>
                    <TableCell>{formatDate(tx.createdAt)}</TableCell>
                    <TableCell>
                      {tx.senderName} ({tx.senderEmail})
                    </TableCell>
                    <TableCell>
                      {tx.recipientName} ({tx.recipientEmail})
                    </TableCell>
                    <TableCell align="right" sx={{ fontWeight: 'bold' }}>
                      {formatCurrency(tx.amount)}
                    </TableCell>
                    <TableCell>
                      {tx.notes || (
                        <Typography
                          variant="body2"
                          color="text.secondary"
                          fontStyle="italic"
                        >
                          No notes
                        </Typography>
                      )}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
          <TablePagination
            component="div"
            count={totalCount}
            page={page}
            onPageChange={(_, newPage) => setPage(newPage)}
            rowsPerPage={pageSize}
            onRowsPerPageChange={(e) => {
              setPageSize(parseInt(e.target.value, 10));
              setPage(0);
            }}
            rowsPerPageOptions={[5, 10, 25]}
          />
        </Paper>
      )}
    </Container>
  );
}
