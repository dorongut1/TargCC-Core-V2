import React from 'react';
import { Routes, Route, Link } from 'react-router-dom';
import { AppBar, Toolbar, Typography, Container, Box, Drawer, List, ListItem, ListItemButton, ListItemText } from '@mui/material';
import { CustomerList } from './components/Customer/CustomerList';
import { CustomerForm } from './components/Customer/CustomerForm';
import { OrderList } from './components/Order/OrderList';
import { OrderForm } from './components/Order/OrderForm';
import { OrderItemList } from './components/OrderItem/OrderItemList';
import { OrderItemForm } from './components/OrderItem/OrderItemForm';
import { ProductList } from './components/Product/ProductList';
import { ProductForm } from './components/Product/ProductForm';

const drawerWidth = 240;

function App() {
  return (
    <Box sx={{ display: 'flex' }}>
      <AppBar position="fixed" sx={{ zIndex: (theme) => theme.zIndex.drawer + 1 }}>
        <Toolbar>
          <Typography variant="h6" noWrap component="div">
            Customer Admin
          </Typography>
        </Toolbar>
      </AppBar>
      <Drawer
        variant="permanent"
        sx={{
          width: drawerWidth,
          flexShrink: 0,
          '& .MuiDrawer-paper': { width: drawerWidth, boxSizing: 'border-box' },
        }}
      >
        <Toolbar />
        <Box sx={{ overflow: 'auto' }}>
          <List>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/customers">
                <ListItemText primary="Customers" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/orders">
                <ListItemText primary="Orders" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/orderItems">
                <ListItemText primary="OrderItems" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/products">
                <ListItemText primary="Products" />
              </ListItemButton>
            </ListItem>
          </List>
        </Box>
      </Drawer>
      <Box component="main" sx={{ flexGrow: 1, p: 3 }}>
        <Toolbar />
        <Container maxWidth="xl">
          <Routes>
            <Route path="/" element={<Typography variant="h4">Welcome</Typography>} />
            <Route path="/customers" element={<CustomerList />} />
            <Route path="/customers/new" element={<CustomerForm />} />
            <Route path="/customers/:id" element={<CustomerForm />} />
            <Route path="/orders" element={<OrderList />} />
            <Route path="/orders/new" element={<OrderForm />} />
            <Route path="/orders/:id" element={<OrderForm />} />
            <Route path="/orderItems" element={<OrderItemList />} />
            <Route path="/orderItems/new" element={<OrderItemForm />} />
            <Route path="/orderItems/:id" element={<OrderItemForm />} />
            <Route path="/products" element={<ProductList />} />
            <Route path="/products/new" element={<ProductForm />} />
            <Route path="/products/:id" element={<ProductForm />} />
          </Routes>
        </Container>
      </Box>
    </Box>
  );
}

export default App;
